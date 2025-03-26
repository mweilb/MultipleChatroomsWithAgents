using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using MultiAgents.AzureAISpeech;
using MultiAgents.AgentsChatRoom.WebSockets;
using Microsoft.SemanticKernel;


namespace MultiAgents.WebSockets
{
    /// <summary>
    /// Handles WebSocket connections and dispatches incoming messages to registered command handlers.
    /// </summary>
    public class WebSocketHandler
    {
        // Dictionary mapping command actions to their respective handlers.
        private readonly ConcurrentDictionary<string, Func<WebSocketBaseMessage, WebSocket,Kernel, IAgentSpeech, Task>> commandHandlers = new();

        /// <summary>
        /// Registers a command handler for a specific action.
        /// </summary>
        /// <param name="action">The action name to register.</param>
        /// <param name="commandHandler">The function to handle the command.</param>
        public void RegisterCommand(string action, Func<WebSocketBaseMessage, WebSocket, Kernel, IAgentSpeech, Task> commandHandler)
        {
            commandHandlers[action] = commandHandler;
        }

       

        /// <summary>
        /// Listens for incoming WebSocket messages and dispatches them to the appropriate command handler.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task HandleRequestAsync(WebSocket webSocket,Kernel kernel, IAgentSpeech agentSpeech)
        {
            // Buffer for receiving incoming messages.
            var buffer = new byte[1024 * 4];

            try
            {
                // Receive the first message.
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // Continue reading until the WebSocket is closed.
                while (!result.CloseStatus.HasValue)
                {
                    // Decode the received bytes into a JSON string.
                    string messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    WebSocketBaseMessage? incomingMessage;
                    try
                    {
                        // Deserialize the JSON into a WebSocketBaseMessage.
                        incomingMessage = JsonSerializer.Deserialize<WebSocketBaseMessage>(messageJson);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error parsing JSON: {ex.Message}");
                        await SendErrorAsync(webSocket, "Invalid JSON format.");
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        continue;
                    }

                    // Validate the message structure.
                    if (incomingMessage == null || string.IsNullOrEmpty(incomingMessage.Action))
                    {
                        await SendErrorAsync(webSocket, "Invalid message format: 'action' is required.");
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        continue;
                    }

                    try
                    {
                        // If a handler is registered for the incoming action, invoke it.
                        if (commandHandlers.TryGetValue(incomingMessage.Action, out var handler))
                        {
                            await handler( incomingMessage, webSocket, kernel,  agentSpeech);
                        }
                        else
                        {
                            // If no handler is registered, send an "unknown action" error message.
                            var unknownResponse = new WebSocketReplyChatRoomMessage
                            {
                                UserId = incomingMessage.UserId,
                                TransactionId = incomingMessage.TransactionId,
                                Action = "unknown",
                                Content = $"Unknown action: {incomingMessage.Action}"
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

                    // Continue receiving the next message.
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                // Close the WebSocket connection gracefully.
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                Console.WriteLine("WebSocket connection closed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an error message over the WebSocket connection.
        /// </summary>
        /// <param name="webSocket">The WebSocket connection.</param>
        /// <param name="errorMessage">The error message to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
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
    }
}
