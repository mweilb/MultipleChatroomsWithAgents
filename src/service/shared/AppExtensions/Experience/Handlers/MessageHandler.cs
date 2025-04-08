using AppExtensions.AISpeech;
using DocumentFormat.OpenXml.Wordprocessing;
 
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.Orchestrator;
 
using System.Net.WebSockets;
 
using WebSocketMessages;
using WebSocketMessages.Messages;
using WebSocketMessages.Messages.Rooms;
using static AppExtensions.Experience.ExperienceManager;

namespace AppExtensions.Experience.Handlers
{
    public class MessageHandler(TrackingInfo info,string name)
    {
        private readonly ILogger<MessageHandler>? logger = null;
     
        private readonly string _name = name;
        private readonly TrackingInfo _trackingInfo = info;

        public async Task HandleCommandAsync(WebSocketBaseMessage message,WebSocket webSocket, ConnectionMode mode)
        {
            // Wrap the WebSocket connection with a sender helper to simplify sending messages.
            var sender = new WebSocketSender(webSocket);

            // Create a cancellation token to manage operation lifetime.
            using var cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;


            var orchestrator = _trackingInfo.agentGroupChatOrchestrator;

            // Ensure that the chat room is properly initialized.
            if (orchestrator == null)
            {
                logger?.LogError("ChatRoom not initialized for {CommandName}", _name);
                await sender.SendError(message.UserId, _name, "handler", $"ChatRoom not initialized {_name}");
                return;
            }

            try
            {
                // Add the user's incoming message to the conversation history.
                orchestrator.AddChatMessage(message.Content);

                WebSocketReplyChatRoomMessage? roomMessage = null;

                await foreach (var streamingContent in orchestrator.InvokeStreamingAsync(cancellationToken))
                {
                    if (streamingContent == null)
                    {
                        // Handle the case where streaming content is null.
                        logger?.LogWarning("Received null streaming content for {CommandName}", _name);
                        continue;
                    }

                    if (streamingContent.Action == StreamingOrchestratorContent.ActionTypes.AgentStarted)
                    {
                        roomMessage = CreateNewMessage(message.UserId, message.Action);
                        roomMessage.AgentName = streamingContent.AgentName;
                        roomMessage.Content = streamingContent.Content?.ToString() ?? "";
                        await sender.SendAsync(roomMessage, mode, cancellationToken);
                        Console.WriteLine($"New Agent {streamingContent.Content?.ToString()}");
                    }
                    else if (streamingContent.Action == StreamingOrchestratorContent.ActionTypes.AgentUpdated)
                    {
                        Console.WriteLine($"Updated {streamingContent.Content?.ToString()}");
                        if (roomMessage != null)
                        {
                            roomMessage.Content += streamingContent.Content?.ToString() ?? "";
                            await sender.SendAsync(roomMessage, mode, cancellationToken);
                        }
                    }
                    else if (streamingContent.Action == StreamingOrchestratorContent.ActionTypes.AgentFinsihed)
                    {
                        Console.WriteLine($"End Agent {streamingContent.Content?.ToString()}");
                    }
                    else if (streamingContent.Action == StreamingOrchestratorContent.ActionTypes.RoomChange)
                    {
                        roomMessage = CreateNewMessage(message.UserId, message.Action);
                        roomMessage.AgentName = "Room Change";
                        roomMessage.Content = $"Request to Change to {streamingContent.Content?.ToString() ?? ""}";
                        await sender.SendAsync(roomMessage, mode, cancellationToken);
                        Console.WriteLine($"Change Room {streamingContent.Content?.ToString()}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log any exceptions and send an error response to the client.
                logger?.LogError(ex, "Error occurred handling command {CommandName}", _name);
                await sender.SendError(message.UserId, _name, "message processing", $"Initialization or logic error: {ex.Message}");
            }
 
        }

        public static WebSocketReplyChatRoomMessage CreateNewMessage(
             string userId,
             string command
         ) => new()
         {
             UserId = userId,
             TransactionId = Guid.NewGuid().ToString(),
             Action = command,
             SubAction = "chunk",
             Content = string.Empty,
             AgentName = "Unknown",
         };



    }
}
