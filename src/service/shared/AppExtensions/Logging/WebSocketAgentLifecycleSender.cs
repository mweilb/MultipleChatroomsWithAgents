using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using AppExtensions.Logging.Aggregators;
using WebSocketMessages.Messages;     // For WebSocketBaseMessage (ensure this class is defined in your project)

namespace WebSocketMessages.AgentLifecycle
{
    /// <summary>
    /// Responsible for sending lifecycle and chat messages over a WebSocket.
    /// Subscribes to aggregator events so it remains decoupled from logging functionality.
    /// </summary>
    public class WebSocketAgentLifecycleSender
    {
        public static string OrchestratorName { get; set; } = "DefaultOrchestrator";
        private WebSocket? _webSocket;
        private readonly string _userId;

        public ConnectionMode CurrentConnectionMode { get; set; } = ConnectionMode.App;


        /// <summary>
        /// Gets or sets the WebSocket instance used to send messages.
        /// </summary>
        public WebSocket? WebSocket
        {
            get => _webSocket;
            set => _webSocket = value;
        }

        /// <summary>
        /// Initializes a new instance of the sender.
        /// Subscribes to lifecycle, regular chat, and streaming chat aggregator events.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        public WebSocketAgentLifecycleSender(string userId)
        {
            _userId = userId;

            // Subscribe to aggregator events.
            AgentLifecycleEventAggregator.OnAgentLifecycleEvent += GlobalLifecycleEventHandler;
            ChatMessageAggregator.OnChatMessageEvent += GlobalChatMessageHandler;
            StreamingChatMessageAggregator.OnStreamingChatMessageEvent += GlobalStreamingChatMessageHandler;
        }

        /// <summary>
        /// Global handler for lifecycle events.
        /// Converts the lifecycle event into a WebSocket message and sends it.
        /// </summary>
        private async void GlobalLifecycleEventHandler(AgentLifecycleEventData eventData)
        {
            //The app does not need log eventgs
            if (CurrentConnectionMode == ConnectionMode.App) { return; }
  
            try
            {
                if (WebSocket == null)
                {
                    Console.WriteLine("WebSocket is null; skipping sending lifecycle event.");
                    return;
                }
                await SendAsync(eventData, _userId, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending lifecycle event: {ex}");
            }
        }

        /// <summary>
        /// Global handler for regular chat message events.
        /// Converts the event data into a WebSocket message and sends it.
        /// </summary>
        private async void GlobalChatMessageHandler(ChatMessageEventData eventData)
        {
            //The app does not need log eventgs
            if (CurrentConnectionMode == ConnectionMode.App) { return; }

            try
            {
                if (WebSocket == null)
                {
                    Console.WriteLine("WebSocket is null; skipping sending chat message.");
                    return;
                }
                await SendChatMessageAsync(eventData, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending chat message: {ex}");
            }
        }

        /// <summary>
        /// Global handler for streaming chat message events.
        /// Converts the event data into a WebSocket message and sends it.
        /// </summary>
        private async void GlobalStreamingChatMessageHandler(StreamingChatMessageEventData eventData)
        {
            //The app does not need log eventgs
            if (CurrentConnectionMode == ConnectionMode.App) { return; }

            try
            {
                if (WebSocket == null)
                {
                    Console.WriteLine("WebSocket is null; skipping sending streaming chat message.");
                    return;
                }
                await SendStreamingChatMessageAsync(eventData, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending streaming chat message: {ex}");
            }
        }

        /// <summary>
        /// Sends a lifecycle event message over the WebSocket.
        /// </summary>
        /// <param name="eventData">The lifecycle event data.</param>
        /// <param name="userId">The user identifier.</param>
        public async Task SendAsync(AgentLifecycleEventData eventData, string userId, CancellationToken cancellationToken = default)
        {
            //The app does not need log eventgs
            if (CurrentConnectionMode == ConnectionMode.App) { return; }

            if (WebSocket == null || string.IsNullOrEmpty(OrchestratorName))
            {
                Console.WriteLine("WebSocket not set or OrchestratorName is empty; cannot send lifecycle message.");
                return;
            }
            // Create the WebSocket message.
            var message = new WebSocketBaseMessage
            {
                UserId = userId,
                TransactionId = Guid.NewGuid().ToString(),
                Action = OrchestratorName,
                SubAction = "reply",
                Content = eventData.Message,
                Mode = "Editor"
            };
            await SendMessageAsync(message, cancellationToken);
        }

        /// <summary>
        /// Sends a regular chat message over the WebSocket.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="transactionId">A transaction identifier.</param>
        /// <param name="messageContent">The message content.</param>
        public async Task SendChatMessageAsync(ChatMessageEventData eventData, CancellationToken cancellationToken = default)
        {
            if (WebSocket == null)
            {
                Console.WriteLine("WebSocket is not set; cannot send chat message.");
                return;
            }
            var metadata = eventData?.Content?.Metadata;
            if (metadata != null)
            {
                if (metadata.TryGetValue("think", out var think))
                {
                    var wsMessage = new WebSocketBaseMessage
                    {
                        UserId = _userId,
                        TransactionId = Guid.NewGuid().ToString(),
                        Action = OrchestratorName,
                        SubAction = $"ChatMessage:{eventData?.EventName ?? ""}",
                        Content = "[Rationale] " + think as string,
                        Mode = "Editor"

                    };
                    await SendMessageAsync(wsMessage, cancellationToken);
                }
                
            }


            string? message = eventData?.Content?.Content;

            if (message != null)
            {
                var wsMessage = new WebSocketBaseMessage
                {
                    UserId = _userId,
                    TransactionId = Guid.NewGuid().ToString(),
                    Action = OrchestratorName,
                    SubAction = $"ChatMessage:{eventData?.EventName ?? ""}",
                    Content = "[GetChatMessageContentAsync] " + message,
                    Mode = "Editor"

                };
                await SendMessageAsync(wsMessage, cancellationToken);
            }
        }

        /// <summary>
        /// Sends a streaming chat message over the WebSocket.
        /// </summary>
        /// <param name="eventName">The event name.</param>
        /// <param name="transactionId">A transaction identifier.</param>
        /// <param name="messageContent">The message content.</param>
        public async Task SendStreamingChatMessageAsync(StreamingChatMessageEventData eventData, CancellationToken cancellationToken = default)
        {
            if (WebSocket == null)
            {
                Console.WriteLine("WebSocket is not set; cannot send streaming chat message.");
                return;
            }

            string? message = eventData?.Content?.Content;
            if (message != null)
            {
                var wsMessage = new WebSocketBaseMessage
                {
                    UserId = _userId,
                    TransactionId = eventData?.TransactionId ?? Guid.NewGuid().ToString(),
                    Action = OrchestratorName,
                    SubAction = $"StreamingChatMessage:{eventData?.EventName ?? ""}",
                    Content = message,
                    Mode = "Editor"
                };
                await SendMessageAsync(wsMessage, cancellationToken);
            }
        }

        /// <summary>
        /// Helper method that serializes a WebSocketBaseMessage and sends it over the WebSocket.
        /// </summary>
        private async Task SendMessageAsync(WebSocketBaseMessage message, CancellationToken cancellationToken)
        {
            // Ensure WebSocket is not null by assigning to a local variable.
            var ws = WebSocket;
            if (ws == null)
            {
                Console.WriteLine("WebSocket is null; cannot send message.");
                return;
            }
            var options = new JsonSerializerOptions { WriteIndented = false, IncludeFields = true };
            string json = JsonSerializer.Serialize(message, options);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, endOfMessage: true, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
