 
using Microsoft.Extensions.Logging;
using SemanticKernelExtension.Orchestrator;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
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
        /// Handles the "rooms/change" subcommand: changes the current room.
        /// </summary>
        public async Task HandleChangeRoomRequestAsync(
            WebSocketBaseMessage message,
            WebSocket webSocket,
            ConnectionMode mode)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<JsonContentPayLoadIForChangeRoom>(message.Content);
                if (payload != null)
                {
                    var chatRoomGroup = _trackingInfo.agentGroupChatOrchestrator;
                    if (chatRoomGroup == null)
                    {
                        await SendErrorAsync(webSocket, "change", $"No chat room group found for: {payload.Group}");
                        return;
                    }

                    // e.g., chatRoomGroup.ChangeRoom(payload.To);
                    // If your chatRoomGroup has a method for changing a room:
                    chatRoomGroup.UserRequestSwitchTo(payload.To);
 
                    message.Action = this._name;

                    await ProcessMessage(message,mode, new WebSocketSender(webSocket), chatRoomGroup, CancellationToken.None);

                    return;

                }
                await SendErrorAsync(webSocket, "change", "Room change payload invalid or group not found.");
            }
            catch (Exception ex)
            {
                await SendErrorAsync(webSocket, "change", $"Exception: {ex.Message}");
            }
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

                await ProcessMessage(message, mode, sender, orchestrator, cancellationToken);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred handling command {CommandName}", _name);
                await sender.SendError(message.UserId, _name, "message processing", $"Initialization or logic error: {ex.Message}");
            }
        }

        public async Task ProcessMessage(WebSocketBaseMessage message, ConnectionMode mode, WebSocketSender sender, AgentGroupChatOrchestrator orchestrator, CancellationToken cancellationToken)
        {
            // Variables to hold reply messages for the chat room.
            WebSocketReplyChatRoomMessage? roomMessage = null;
            WebSocketReplyChatRoomMessage? rationaleMessage = null;

            // Set global orchestrator name for lifecycle reporting.
            WebSocketAgentLifecycleSender.OrchestratorName = orchestrator.Name;
            bool yieldOnRoom = false;
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
                    yieldOnRoom = streamingContent.YieldOnRoomChange;
                }
                // If the event is AgentFinished or RoomMessageFinished, do nothing.
            }

        

            if (yieldOnRoom == false)
            {
                SendCompleteRequest(sender, message.UserId, orchestrator.Name, cancellationToken);
            }

            // Clear global orchestrator name.
            WebSocketAgentLifecycleSender.OrchestratorName = string.Empty;
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

            var trackInfo = _trackingInfo.VisualInfoPerName?[streamingContent.ChatName];
            VisualInfo? visualInfo = null;
            if (trackInfo != null)
            {
                trackInfo.TryGetValue(streamingContent.AgentName, out visualInfo);
            }
            if (mode != ConnectionMode.App)
            {
                // Create a rationale message for the editor.
                var rationaleMsg = CreateNewMessage(originalMessage.UserId, streamingContent.OrchestratorName);
                rationaleMsg.AgentName = streamingContent.AgentName;
                rationaleMsg.Orchestrator = streamingContent.OrchestratorName;
                rationaleMsg.RoomName = streamingContent.ChatName;
                rationaleMsg.Content = hintInfo;
                rationaleMsg.Mode = "Editor";
                rationaleMsg.Emoji = visualInfo?.Emoji ?? "";
                rationaleMsg.DisplayName = visualInfo?.DisplayName ?? streamingContent.AgentName ?? "";

                if (!string.IsNullOrWhiteSpace(hintInfo))
                {
                    await sender.SendAsync(rationaleMsg, mode, cancellationToken);
                }
            }

            // Create the room message using content info.
            string contentInfo = streamingContent.Content?.ToString() ?? "";
            var roomMsg = CreateNewMessage(originalMessage.UserId, streamingContent.OrchestratorName);
            roomMsg.AgentName = streamingContent.AgentName ?? "Not Set Agent Name";
            roomMsg.Orchestrator = streamingContent.OrchestratorName;
            roomMsg.RoomName = streamingContent.ChatName;

            roomMsg.Content = contentInfo;
            roomMsg.Emoji = visualInfo?.Emoji ?? "";
            roomMsg.DisplayName = visualInfo?.DisplayName ?? streamingContent.AgentName ?? "";

            if (!string.IsNullOrWhiteSpace(contentInfo))
            {
                await sender.SendAsync(roomMsg, mode, cancellationToken);
            }

            return (roomMsg, roomMsg);
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
                if (rationaleMsg != null && mode == ConnectionMode.Editor)
                {
                    string hintInfo = GetHint(streamingContent);
                    if (!string.IsNullOrWhiteSpace(hintInfo))
                    {
                        rationaleMsg.Content += hintInfo;
                        rationaleMsg.Mode = "Editor";
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
            var roomMsg = CreateChangeRoomMessage(originalMessage.UserId, originalMessage.Action);
            roomMsg.AgentName = "Room Change";
            roomMsg.SubAction = streamingContent.YieldOnRoomChange ? "change-room-yield" : "change-room";
            roomMsg.To = streamingContent.Content?.ToString() ?? "";
            roomMsg.From = streamingContent.ChatName;
            roomMsg.Content = (!streamingContent.YieldOnRoomChange ? "Auto Change " : "Request ") + $"to '{roomMsg.To}' from '{roomMsg.From}'";
            roomMsg.DisplayName = roomMsg.AgentName;

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
                SubAction = "reply",
                Content = string.Empty,
                AgentName = "Unknown"
            };

        /// Creates a new WebSocket reply message for the chat room.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="command">The command or action name.</param>
        private static async void SendCompleteRequest(WebSocketSender sender, string userId, string command, CancellationToken cancellationToken) { 
            
            var completeMessage = new WebSocketReplyChatRoomMessage
            {
                UserId = userId,
                TransactionId = Guid.NewGuid().ToString(),
                Action = command,
                SubAction = "completed",
                Content = "completed",
                AgentName = "Unknown",
                Mode = "App",
                DisplayName = command
            };

              await sender.SendAsync(completeMessage, ConnectionMode.App,cancellationToken);

                
        }



        /// <summary>
        /// Creates a new WebSocket reply message for the chat room.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="command">The command or action name.</param>
        public static WebSocketChangeRoom CreateChangeRoomMessage(
            string userId,
            string command) => new WebSocketChangeRoom()
            {
                UserId = userId,
                TransactionId = Guid.NewGuid().ToString(),
                Action = command,
                SubAction = "reply",
                AgentName = "Unknown",
              
            };


        /// <summary>
        /// Sends an error message over the WebSocket.
        /// </summary>
        private async Task SendErrorAsync(WebSocket webSocket, string? subAction, string errorMessage)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                SubAction = subAction ?? string.Empty,
                Content = errorMessage
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
            await webSocket.SendAsync(
                new ArraySegment<byte>(errorJson),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }


    }
}
