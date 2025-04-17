using DocumentFormat.OpenXml.ExtendedProperties;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketMessages;
using WebSocketMessages.Messages;
using WebSocketMessages.Messages.Rooms;
using YamlConfigurations.FileReader;


namespace AppExtensions.Experience.Handlers
{

 
    /// <summary>
    /// Handles all "rooms" commands: get, change, reset, etc.
    /// </summary>
    public class RoomsHandler(ExperienceManager experienceManager)
    {
        private readonly ExperienceManager _manager = experienceManager;
 
        /// <summary>
        /// The entry point that the WebSocket command dispatcher calls for "rooms" commands.
        /// Dispatches to the appropriate sub-handler method based on SubAction.
        /// </summary>
        public async Task HandleRoomsCommandAsync(
            WebSocketBaseMessage message,
            WebSocket webSocket,
            ConnectionMode mode
            /* If you need an IAgentSpeech object, you can inject it here or from the constructor.*/)
        {
            try
            {
                switch (message.SubAction?.ToLowerInvariant())
                {
                    case "get":
                        await HandleGetRequestAsync(message, webSocket, mode);
                        break;

                    case "reset":
                        // If you need speech, pass it in from the constructor or arguments
                        // await HandleResetChatRequestAsync(message, webSocket, mode, speech);
                        await HandleResetChatRequestAsync(message, webSocket, mode);
                        break;

                    default:
                        // Unknown SubAction
                        await SendErrorAsync(webSocket, message.SubAction,
                            $"Unknown SubAction: {message.SubAction}");
                        break;
                }
            }
            catch (Exception ex)
            {
                await SendErrorAsync(webSocket, message.SubAction, $"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the "rooms/get" subcommand: lists all available rooms.
        /// </summary>
        private async Task HandleGetRequestAsync(
            WebSocketBaseMessage message,
            WebSocket webSocket,
            ConnectionMode mode)
        {
            var response = new WebSocketGetRoomsMessage
            {
                UserId = "system",
                TransactionId = message.TransactionId,
                Action = "rooms",
                SubAction = "room list",
                Content = "List of available rooms"
            };

            foreach (var (name, experience) in _manager.Experiences)
            {
                var group = experience.Experience;
                if (group == null)
                {
                    continue;
                }

                var rooms = group.Rooms?.Select(
                    kvp => new WebSocketRoomProfile
                    {
                        Name = kvp.Value.Name,
                        Emoji = kvp.Value.Emoji,
                        Agents = kvp.Value.Agents?.Select(agent => new WebSocketAgentProfile
                        {
                            Name = agent.Name,
                            Emoji = agent.Emoji??string.Empty
                        }).ToList() ?? []
                    })
                ?? [];

                // Convert YAML errors to WebSocketValidationError objects before constructing the response
                List<WebSocketValidationError> wsErrors = group.Errors?
                    .Select(e => new WebSocketValidationError
                    {
                        Message = e.Message,
                        Location = e.Location,
                        LineNumber = e.LineNumber,
                        CharPosition = e.CharPosition
                    }).ToList() ?? new List<WebSocketValidationError>();

                if (mode == ConnectionMode.App)
                {
                    response.Rooms.Add(new WebSocketGetRooms
                    {
                        Name = name,
                        Emoji = group.Emoji,
                        MerMaidGraph = "",
                        Yaml = "",
                        Errors = [],
                        AutoStart = group.AutoStart,
                        Rooms = [.. rooms]
                    });
                }
                else
                {
                    response.Rooms.Add(new WebSocketGetRooms
                    {
                        Name = name,
                        Emoji = group.Emoji,
                        MerMaidGraph = MermaidGenerator.GenerateMermaidDiagram(group),
                        Yaml = group.Yaml,
                        Errors = wsErrors,
                        AutoStart = group.AutoStart,
                        Rooms = [.. rooms]
                    });
                }
            }

            var responseJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await webSocket.SendAsync(
                new ArraySegment<byte>(responseJson),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: CancellationToken.None
            );
        }

     
        /// <summary>
        /// Handles the "rooms/reset" subcommand: resets a chat room.
        /// </summary>
        private async Task HandleResetChatRequestAsync(
            WebSocketBaseMessage message,
            WebSocket webSocket,
            ConnectionMode mode
            /*, IAgentSpeech speech*/)
        {
            try
            {
                string roomToReset = message.Content;
                if (string.IsNullOrEmpty(roomToReset))
                {
                    await SendErrorAsync(webSocket, "reset", "No room specified for reset.");
                    return;
                }

                if (_manager.Experiences.TryGetValue(roomToReset, out var expereience))
                {
                    // If your group supports a reset:
                    bool resetSucceeded = false;
                    if (expereience.agentGroupChatOrchestrator != null)
                    {
                        resetSucceeded = await expereience.agentGroupChatOrchestrator.ResetAsync();
                    }

                    var chatRoomGroup = expereience.Experience;
                    if (!resetSucceeded || chatRoomGroup == null)
                    {
                        await SendErrorAsync(webSocket, "reset", $"Failed to reset chat for room: {roomToReset}");
                        return;
                    }

                    
                  
                    // If AutoStart is set, you can perform any additional logic:
                    bool autoStart = string.Equals(chatRoomGroup.AutoStart, "yes", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(chatRoomGroup.AutoStart, "true", StringComparison.OrdinalIgnoreCase);
                    if (autoStart)
                    {
                     

                        if (_manager.Experiences.TryGetValue(chatRoomGroup.Name, out var tracking)){
                           
                            
                            var agentGroupChatOrchestrator = tracking.agentGroupChatOrchestrator;
                            var messageHandler = tracking.handler;
                            if ((agentGroupChatOrchestrator != null)  && (messageHandler != null))
                            {
                                var sender = new WebSocketSender(webSocket);
                                using var cts = new CancellationTokenSource();
                                CancellationToken cancellationToken = cts.Token;
                                message.Action = chatRoomGroup.Name;
                                message.UserId = "system";
                                await messageHandler.ProcessMessage(message, mode, sender, agentGroupChatOrchestrator, cancellationToken);
                            }
                          
                        }
                        
                       

                        
                    }

                    // Otherwise, confirm success:
                    var successResponse = new WebSocketBaseMessage
                    {
                        Action = "rooms",
                        SubAction = "reset",
                        Content = $"Successfully reset room: {roomToReset}"
                    };
                    var successJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(successResponse));
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(successJson),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                else
                {
                    await SendErrorAsync(webSocket, "reset", $"Room not found: {roomToReset}");
                }
            }
            catch (Exception ex)
            {
                await SendErrorAsync(webSocket, "reset", $"Exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Sends an error message over the WebSocket.
        /// </summary>
        private async Task SendErrorAsync(WebSocket webSocket, string? subAction, string errorMessage)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                SubAction = subAction??string.Empty,
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
