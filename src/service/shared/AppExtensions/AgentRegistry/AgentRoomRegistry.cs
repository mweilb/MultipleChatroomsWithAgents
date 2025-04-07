using AppExtensions.AISpeech;
using Microsoft.SemanticKernel;

using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using WebSocketMessages;
using WebSocketMessages.Messages;
using WebSocketMessages.Messages.Rooms;
using YamlConfigurations;
using YamlConfigurations.FileReader;
using YamlConfigurations.Librarians;

namespace AppExtensions.AgentRegistry
{
    public class AgentRoomRegistry
    {
        // Dictionary to store chat room groups.
        private readonly Dictionary<string, YamlMultipleChatRooms> dictMultipleChartRooms = new();

        // LibrarianRegistry encapsulates all librarian-related functionality.
        private readonly LibrarianRegistry librarianRegistry = new();

        /// <summary>
        /// Appends additional chat rooms to the registry.
        /// </summary>
        public void AppendRooms(Dictionary<string, YamlMultipleChatRooms> moreRooms)
        {
            foreach (var kvp in moreRooms)
            {
                if (!dictMultipleChartRooms.ContainsKey(kvp.Key))
                {
                    dictMultipleChartRooms.Add(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// Appends additional librarian groups by delegating to LibrarianRegistry.
        /// </summary>
        public void AppendLibrarians(Dictionary<string, YamLibrarians> moreLibrarians)
        {
            librarianRegistry.AppendLibrarians(moreLibrarians);
        }

        /// <summary>
        /// Registers WebSocket command handlers for both rooms and librarians.
        /// </summary>
        public void RegisterHandlers(WebSocketHandler webSocketHandler)
        {
            webSocketHandler.RegisterCommand("rooms", HandleRoomsCommandAsync);
            // Delegate librarian command handling to LibrarianRegistry.
            webSocketHandler.RegisterCommand("librarians", (msg, socket, mode) => librarianRegistry.HandleLibrariansCommandAsync(msg, socket, mode));

            foreach (var (name, group) in dictMultipleChartRooms)
            {
             //   group.RegisterChatRooms(name, group.StartRoom, webSocketHandler);
            }
        }

        /// <summary>
        /// Handles the "rooms" command by delegating to specific methods based on SubAction.
        /// </summary>
        private async Task HandleRoomsCommandAsync(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode)
        {
            if (message.SubAction == "get")
            {
                await HandleGetRequestAsync(message, webSocket, mode);
            }
            else if (message.SubAction == "change")
            {
                await HandleChangeRoomRequestAsync(message, webSocket, mode);
            }
            else if (message.SubAction == "reset")
            {
              //  await HandleResetChatRequestAsync(message, webSocket, mode, speech);
            }
        }

        /// <summary>
        /// Handles the "get" command for chat rooms.
        /// </summary>
        private async Task HandleGetRequestAsync(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode)
        {
            var response = new WebSocketGetRoomsMessage
            {
                UserId = "system",
                TransactionId = message.TransactionId,
                Action = "rooms",
                SubAction = "room list",
                Content = "List of available rooms"
            };

            foreach (var (name, group) in dictMultipleChartRooms)
            {
                response.Rooms.Add(new WebSocketGetRooms
                {
                    Name = name,
                    Emoji = group.Emoji,
                    MerMaidGraph = MermaidGenerator.GenerateMermaidDiagram(group),
                    Yaml = group.Yaml,
                    Errors = group.Errors,
                    AutoStart = group.AutoStart,
                    Rooms = group.Rooms != null
                        ? group.Rooms.Select(room => new WebSocketRoomProfile
                        {
                            Name = room.Value.Name,
                            Emoji = room.Value.Emoji,
                            Agents = room.Value.Agents != null
                                ? room.Value.Agents.Select(agent => new WebSocketAgentProfile
                                {
                                    Name = agent.Name,
                                    Emoji = agent.Emoji
                                }).ToList()
                                : new List<WebSocketAgentProfile>()
                        }).ToList()
                        : new List<WebSocketRoomProfile>()
                });
            }

            var responseJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
            await webSocket.SendAsync(
                new ArraySegment<byte>(responseJson),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }

        /// <summary>
        /// Handles the room change command.
        /// </summary>
        public async Task HandleChangeRoomRequestAsync(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode)
        {
            try
            {
                var payload = JsonSerializer.Deserialize<JsonContentPayLoadIForChangeRoom>(message.Content);
                if (payload != null)
                {
                    if (dictMultipleChartRooms.TryGetValue(payload.Group, out var chatRoomGroup))
                    {
                      // TODO  if (chatRoomGroup.ChangeRoom(payload.To) == true)
                        {
                            return;
                        }
                    }
                }
                await SendErrorAsync(webSocket, "change", "did not change room");
            }
            catch (Exception e)
            {
                await SendErrorAsync(webSocket, "change", e.Message.ToString());
            }
        }

        /// <summary>
        /// Handles the reset chat command.
        /// </summary>
        private async Task HandleResetChatRequestAsync(WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode, IAgentSpeech speech)
        {
            try
            {
                string roomToReset = message.Content;
                if (string.IsNullOrEmpty(roomToReset))
                {
                    await SendErrorAsync(webSocket, "reset", "No room specified for reset");
                    return;
                }

                if (dictMultipleChartRooms.TryGetValue(roomToReset, out var chatRoomGroup))
                {
                    bool resetSucceeded = false;// await chatRoomGroup.ResetAsync();


                    bool AutoStart = string.CompareOrdinal(chatRoomGroup.AutoStart, "yes") == 0 ||
                       string.CompareOrdinal(chatRoomGroup.AutoStart, "true") == 0;
                    

                    if (AutoStart)
                    {
                       // T await chatRoomGroup.SendMessageToRoomAsync("start", message, webSocket, speech, mode);
                    }
                   

                    if (!resetSucceeded)
                    {
                        await SendErrorAsync(webSocket, "reset", $"Failed to reset chat for room {roomToReset}");
                    }
                }
                else
                {
                    await SendErrorAsync(webSocket, "reset", $"Room {roomToReset} not found");
                }


            }
            catch (Exception e)
            {
                await SendErrorAsync(webSocket, "reset", e.Message);
            }
        }

        /// <summary>
        /// Sends an error message over the WebSocket.
        /// </summary>
        private async Task SendErrorAsync(WebSocket webSocket, string subAction, string errorMessage)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                SubAction = subAction,
                Content = errorMessage
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
            await webSocket.SendAsync(
                new ArraySegment<byte>(errorJson),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }
}
