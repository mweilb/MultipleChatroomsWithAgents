using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using WebSocketMessages.Messages;
using WebSocketMessages.Messages.Rooms;
using System.IO;
using System.Threading;

namespace WebSocketMessages
{
    public enum ConnectionMode
    {
        Editor,
        App
    }

    /// <summary>
    /// Handles WebSocket connections and dispatches incoming messages to registered command handlers.
    /// </summary>
    public class WebSocketHandler
    {
        // Dictionary mapping command actions to their respective handlers.
        private readonly ConcurrentDictionary<string, Func<WebSocketBaseMessage, WebSocket, ConnectionMode, Task>> commandHandlers = new();

        // Semaphore to lock WebSocket message processing
        private readonly SemaphoreSlim _wsMessageSemaphore = new(1, 1);

        // Property to expose lock state
        public bool IsProcessingWebSocketMessage { get; private set; } = false;

        // (YAML config logic removed; now handled externally)

        /// <summary>
        /// Gets or sets the current connection mode.
        /// </summary>
        public ConnectionMode CurrentConnectionMode { get; set; } = ConnectionMode.App;

        public WebSocketHandler()
        {
            // Register the ModeRequest handler.
            RegisterCommand("mode", ModeRequestHandler);
            // Register ping handler
            RegisterCommand("ping", PingHandler);
        }

        /// <summary>
        /// Registers a command handler for a specific action.
        /// </summary>
        /// <param name="action">The action name to register.</param>
        /// <param name="commandHandler">The function to handle the command.</param>
        public void RegisterCommand(string action, Func<WebSocketBaseMessage, WebSocket, ConnectionMode, Task> commandHandler)
        {
            commandHandlers[action] = commandHandler;
        }

        /// <summary>
        /// Listens for incoming WebSocket messages and dispatches them to the appropriate command handler.
        /// </summary>
        // Track connected clients
        private readonly ConcurrentBag<WebSocket> _connectedClients = new();

        public async Task HandleRequestAsync(WebSocket webSocket)
        {
            _connectedClients.Add(webSocket);
            var buffer = new byte[1024 * 4];

            try
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!result.CloseStatus.HasValue)
                {
                    await _wsMessageSemaphore.WaitAsync();
                    IsProcessingWebSocketMessage = true;
                    try
                    {
                        string messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        WebSocketBaseMessage? incomingMessage;
                        try
                        {
                            incomingMessage = JsonSerializer.Deserialize<WebSocketBaseMessage>(messageJson);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error parsing JSON: {ex.Message}");
                            await SendErrorAsync(webSocket, "Invalid JSON format.");
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            continue;
                        }

                        if (incomingMessage == null || string.IsNullOrEmpty(incomingMessage.Action))
                        {
                            await SendErrorAsync(webSocket, "Invalid message format: 'action' is required.");
                            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                            continue;
                        }

                        try
                        {
                            if (commandHandlers.TryGetValue(incomingMessage.Action, out var handler))
                            {
                                // Pass the current connection mode to the handler.
                                await handler(incomingMessage, webSocket, CurrentConnectionMode);
                            }
                            else
                            {
                                var unknownResponse = new WebSocketReplyChatRoomMessage
                                {
                                    UserId = incomingMessage.UserId,
                                    TransactionId = incomingMessage.TransactionId,
                                    Action = "unknown",
                                    SubAction = incomingMessage.Action,
                                    Content = $"Unknown action: {incomingMessage.Action}",
                                    DisplayName = incomingMessage.UserId
                                };

                                var unknownJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(unknownResponse));
                                await webSocket.SendAsync(new ArraySegment<byte>(unknownJson), WebSocketMessageType.Text, true, CancellationToken.None);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error handling action '{incomingMessage.Action}': {ex.Message}");
                            await SendErrorAsync(webSocket, $"Error processing action '{incomingMessage.Action}'.");
                        }

                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    finally
                    {
                        IsProcessingWebSocketMessage = false;
                        _wsMessageSemaphore.Release();
                    }
                }

                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                Console.WriteLine("WebSocket connection closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
            finally
            {
                // Remove closed socket
                // (ConcurrentBag does not support removal, so this is a limitation;
                // for production, use ConcurrentDictionary or similar for better management)
            }
        }

        public void SendToAllClients(WebSocketConfigReloadedMessage message)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(message);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(bytes);

            foreach (var ws in _connectedClients)
            {
                if (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        ws.SendAsync(segment, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to send to client: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Sends an error message over the WebSocket connection.
        /// </summary>
        private async Task SendErrorAsync(WebSocket webSocket, string errorMessage)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                Content = errorMessage
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
            await webSocket.SendAsync(new ArraySegment<byte>(errorJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Handles a ping request by responding with a pong.
        /// </summary>
        private async Task PingHandler(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode currentMode)
        {
            var pongResponse = new WebSocketBaseMessage
            {
                UserId = message.UserId,
                TransactionId = message.TransactionId,
                Action = "pong",
                SubAction = "",
                Content = "pong",
                Mode = message.Mode,
                Version = message.Version
            };

            var pongJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(pongResponse));
            await webSocket.SendAsync(new ArraySegment<byte>(pongJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        /// <summary>
        /// Handles a ModeRequest by updating the connection mode based on the SubAction.
        /// </summary>
        private async Task ModeRequestHandler(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode currentMode)
        {
            if (message.SubAction.Equals("editor", StringComparison.OrdinalIgnoreCase))
            {
                CurrentConnectionMode = ConnectionMode.Editor;
            }
            else if (message.SubAction.Equals("app", StringComparison.OrdinalIgnoreCase))
            {
                CurrentConnectionMode = ConnectionMode.App;
            }
            else
            {
                await SendErrorAsync(webSocket, "Invalid subaction for ModeRequest. Use 'editor' or 'app'.");
                return;
            }

            // Acknowledge the mode change.
            var response = new WebSocketBaseMessage
            {
                UserId = message.UserId,
                TransactionId = message.TransactionId,
                Action = "ModeResponse",
                SubAction = message.SubAction,
                Content = $"Connection mode updated to {CurrentConnectionMode}",
                Mode = message.Mode,
                Version = message.Version
            };

            var responseJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await webSocket.SendAsync(new ArraySegment<byte>(responseJson), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
