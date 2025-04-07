using AppExtensions.AISpeech;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.Orchestrator;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using WebSocketMessages;
using WebSocketMessages.AgentLifecycle;
using WebSocketMessages.Messages;
using WebSocketMessages.Messages.Rooms;
using static AppExtensions.Experience.ExperienceManager;

namespace AppExtensions.Experience.Handlers
{
    /// <summary>
    /// Handles incoming WebSocket messages and orchestrates command processing.
    /// </summary>
    public class MessageHandler
    {
        private readonly ILogger<MessageHandler>? logger;
        private readonly string _name;
        private readonly TrackingInfo _trackingInfo;

        /// <summary>
        /// Initializes a new instance of MessageHandler.
        /// </summary>
        /// <param name="info">Tracking information.</param>
        /// <param name="name">Command or handler name.</param>
        public MessageHandler(TrackingInfo info, string name)
        {
            _trackingInfo = info;
            _name = name;
            // Ideally the logger is injected via DI; here it may be null.
            logger = null;
        }

        /// <summary>
        /// Processes an incoming command message over a WebSocket.
        /// </summary>
        /// <param name="message">The incoming WebSocket message.</param>
        /// <param name="webSocket">The WebSocket connection.</param>
        /// <param name="mode">The connection mode.</param>
        public async Task HandleCommandAsync(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode)
        {
            // Wrap WebSocket connection with a sender helper for simplified messaging.
            var sender = new WebSocketSender(webSocket);
            using var cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            // Retrieve the orchestrator for the current chatroom.
            var orchestrator = _trackingInfo.agentGroupChatOrchestrator;
            if (orchestrator == null)
            {
                logger?.LogError("ChatRoom not initialized for {CommandName}", _name);
                await sender.SendError(message.UserId, _name, "handler", $"ChatRoom not initialized {_name}");
                return;
            }

            try
            {
                // Add incoming message to conversation history.
                orchestrator.AddChatMessage(message.Content);

                // Variables to hold reply messages for the chat room.
                WebSocketReplyChatRoomMessage? roomMessage = null;
                WebSocketReplyChatRoomMessage? rationaleMessage = null;

                // Set global orchestrator name for lifecycle reporting.
                WebSocketAgentLifecycleSender.OrchestratorName = orchestrator.Name;

                // Process streaming responses from the orchestrator.
                await foreach (var streamingContent in orchestrator.InvokeStreamingAsync(cancellationToken))
                {
                    if (streamingContent == null)
                    {
                        logger?.LogWarning("Received null streaming content for {CommandName}", _name);
                        continue;
                    }

                    // Choose sub-function based on the streaming content action.
                    if (IsStartEvent(streamingContent))
                    {
                        (roomMessage, rationaleMessage) = await HandleStartEventAsync(sender, streamingContent, message, mode, cancellationToken);
                    }
                    else if (IsUpdateEvent(streamingContent))
                    {
                        await HandleUpdateEventAsync(sender, streamingContent, mode, cancellationToken, roomMessage, rationaleMessage);
                    }
                    else if (streamingContent.Action == StreamingOrchestratorContent.ActionTypes.RoomChange)
                    {
                        await HandleRoomChangeEventAsync(sender, streamingContent, message, mode, cancellationToken);
                    }
                    // If the event is AgentFinished or RoomMessageFinished, do nothing.
                }

                // Clear global orchestrator name.
                WebSocketAgentLifecycleSender.OrchestratorName = string.Empty;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred handling command {CommandName}", _name);
                await sender.SendError(message.UserId, _name, "message processing", $"Initialization or logic error: {ex.Message}");
            }
        }

        /// <summary>
        /// Determines if the streaming event is a "start" event.
        /// </summary>
        private static bool IsStartEvent(StreamingOrchestratorContent content) =>
            content.Action == StreamingOrchestratorContent.ActionTypes.AgentStarted ||
            content.Action == StreamingOrchestratorContent.ActionTypes.RoomMessageStarted;

        /// <summary>
        /// Determines if the streaming event is an "update" event.
        /// </summary>
        private static bool IsUpdateEvent(StreamingOrchestratorContent content) =>
            content.Action == StreamingOrchestratorContent.ActionTypes.AgentUpdated ||
            content.Action == StreamingOrchestratorContent.ActionTypes.RoomMessageUpdated;

        /// <summary>
        /// Processes "start" events by creating and sending initial reply messages.
        /// Returns the created room and rationale messages.
        /// </summary>
        private async Task<(WebSocketReplyChatRoomMessage roomMsg, WebSocketReplyChatRoomMessage rationaleMsg)>
            HandleStartEventAsync(
                WebSocketSender sender,
                StreamingOrchestratorContent streamingContent,
                WebSocketBaseMessage originalMessage,
                ConnectionMode mode,
                CancellationToken cancellationToken)
        {
            // Extract hint (editor suggestion) from the streaming content.
            string hintInfo = GetHint(streamingContent);
            // Create a rationale message for the editor.
            var rationaleMsg = CreateNewMessage(originalMessage.UserId, originalMessage.Action);
            rationaleMsg.AgentName = streamingContent.AgentName;
            rationaleMsg.Content = hintInfo;
            rationaleMsg.Mode = "Editor";
            rationaleMsg.Emoji = _trackingInfo.RoomAgentEmojis?[streamingContent.ChatName]?
                .TryGetValue(streamingContent.AgentName ?? "", out var emoji1) == true ? emoji1 : "";

            if (!string.IsNullOrWhiteSpace(hintInfo))
            {
                await sender.SendAsync(rationaleMsg, mode, cancellationToken);
            }

            // Create the room message using content info.
            string contentInfo = streamingContent.Content?.ToString() ?? "";
            var roomMsg = CreateNewMessage(originalMessage.UserId, originalMessage.Action);
            roomMsg.AgentName = streamingContent.AgentName ?? "Not Set Agent Name";
            roomMsg.Content = contentInfo;
            roomMsg.Emoji = _trackingInfo.RoomAgentEmojis?[streamingContent.ChatName]?
                .TryGetValue(streamingContent.AgentName ?? "", out var emoji2) == true ? emoji2 : "";

            if (!string.IsNullOrWhiteSpace(contentInfo))
            {
                await sender.SendAsync(roomMsg, mode, cancellationToken);
            }

            return (roomMsg, rationaleMsg);
        }

        /// <summary>
        /// Processes "update" events by updating and sending existing reply messages.
        /// </summary>
        private async Task HandleUpdateEventAsync(
            WebSocketSender sender,
            StreamingOrchestratorContent streamingContent,
            ConnectionMode mode,
            CancellationToken cancellationToken,
            WebSocketReplyChatRoomMessage? roomMsg,
            WebSocketReplyChatRoomMessage? rationaleMsg)
        {
            if (roomMsg != null)
            {
                // Update rationale message if available.
                if (rationaleMsg != null)
                {
                    string hintInfo = GetHint(streamingContent);
                    if (!string.IsNullOrWhiteSpace(hintInfo))
                    {
                        rationaleMsg.Content += hintInfo;
                        await sender.SendAsync(rationaleMsg, mode, cancellationToken);
                    }
                }
                // Append updated content to the room message.
                string contentInfo = streamingContent.Content?.ToString() ?? "";
                if (!string.IsNullOrWhiteSpace(contentInfo))
                {
                    roomMsg.Content += contentInfo;
                    await sender.SendAsync(roomMsg, mode, cancellationToken);
                }
            }
        }

        /// <summary>
        /// Processes "RoomChange" events by creating and sending a room change message.
        /// </summary>
        private async Task HandleRoomChangeEventAsync(
            WebSocketSender sender,
            StreamingOrchestratorContent streamingContent,
            WebSocketBaseMessage originalMessage,
            ConnectionMode mode,
            CancellationToken cancellationToken)
        {
            var roomMsg = CreateNewMessage(originalMessage.UserId, originalMessage.Action);
            roomMsg.AgentName = "Room Change";
            roomMsg.Content = $"Request to Change to {streamingContent.Content?.ToString() ?? ""}";
            await sender.SendAsync(roomMsg, mode, cancellationToken);
        }

        /// <summary>
        /// Extracts the hint (editor suggestion) from the streaming orchestrator content.
        /// </summary>
        /// <param name="streamingContent">The streaming orchestrator content.</param>
        /// <returns>The hint string if available; otherwise, an empty string.</returns>
        private string GetHint(StreamingOrchestratorContent streamingContent)
        {
            // Look for the key "think" in the content metadata.
            if (streamingContent?.Content?.Metadata != null &&
                streamingContent.Content.Metadata.TryGetValue("think", out var think) &&
                think is string stringThink)
            {
                return stringThink;
            }

            return "";
        }

        /// <summary>
        /// Creates a new WebSocket reply message for the chat room.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="command">The command or action name.</param>
        public static WebSocketReplyChatRoomMessage CreateNewMessage(
            string userId,
            string command) => new WebSocketReplyChatRoomMessage
            {
                UserId = userId,
                TransactionId = Guid.NewGuid().ToString(),
                Action = command,
                SubAction = "chunk",
                Content = string.Empty,
                AgentName = "Unknown"
            };
    }
}
