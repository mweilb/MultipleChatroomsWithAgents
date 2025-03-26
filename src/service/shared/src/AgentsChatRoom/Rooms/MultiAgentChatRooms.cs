
using Microsoft.SemanticKernel;
using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.AzureAISpeech;
using MultiAgents.WebSockets;
using System.Net.WebSockets;
 

namespace MultiAgents.AgentsChatRoom.Rooms
{
    /// <summary>
    /// Central manager for agent handlers. Responsible for assembling and registering agent chat rooms.
    /// This class wires together agent registries, chat room instances, and logging, then registers the commands for WebSocket handling.
    /// </summary>
    public class MultiAgentChatRooms
    {
        public virtual Dictionary<string, IMultiAgentHandler>? GetRooms() { return []; }

        public virtual string Emoji { get; set; } = "";

        // List of registered agent handlers.

        public string Name = "";
        public string CurrentRoomName = "";
 

        /// <summary>
        /// Registers all agent chat room handlers with the provided WebSocketHandler.
        /// Each room's command name is mapped to its HandleCommandAsync callback.
        /// Also registers a special "rooms" command to retrieve a list of available rooms.
        /// </summary>
        /// <param name="webSocketHandler">The WebSocketHandler used for registering commands.</param>
        /// <param name="kernel">The Semantic Kernel instance (passed if needed by commands).</param>
        public void RegisterChatRooms(string name, string startName, WebSocketHandler webSocketHandler)
        {
            Name = name;

            // Register the "rooms" command using the helper function.
            webSocketHandler.RegisterCommand(name, HandleCommandAsync);
            var rooms = GetRooms();
            if (rooms != null)
            {
                if (rooms.TryGetValue(startName, out var handle))
                {
                    CurrentRoomName = startName;
                }
                else if (rooms.Keys.Count > 0)
                {
                    CurrentRoomName = rooms.Keys.FirstOrDefault() ?? "";
                }
            }

        }
 

        private async Task HandleCommandAsync(WebSocketBaseMessage message, WebSocket webSocket,Kernel _, IAgentSpeech speech)
        {
            await SendMessageToRoom("user",message, webSocket, speech);
        }


        public async Task SendMessageToRoom(string user, WebSocketBaseMessage message, WebSocket webSocket, IAgentSpeech speech)
        {
            var rooms = GetRooms();
            if ((rooms != null) && rooms.TryGetValue(CurrentRoomName, out var room))
            {
                var (newChatRoom, chatRoomName, chatRoomContent, transactionID) = await room.HandleCommandAsync(user, message, webSocket, speech);
                if (newChatRoom)
                {
                    await ChangeChatRooms(message, webSocket, speech, chatRoomName, chatRoomContent, transactionID);
                }

            }
            
        }

        public bool ChangeRoom(string to)
        {
            var rooms = GetRooms();
            if (rooms?.ContainsKey(to) == true && CurrentRoomName != to)
            {
                CurrentRoomName = to;
                return true;
            }
            return false;
        }

        private async Task ChangeChatRooms(WebSocketBaseMessage message, WebSocket webSocket, IAgentSpeech speech, string toChatRoomName, string chatRoomContent, string transactionID)
        {
            var rooms = GetRooms();
            if ((rooms != null) && rooms.TryGetValue(toChatRoomName, out var room))
            {
                var fromChatRoomName = CurrentRoomName;
                CurrentRoomName = toChatRoomName;

                // Create a cancellation token to manage operation lifetime.
                using var cts = new CancellationTokenSource();
                CancellationToken cancellationToken = cts.Token;

                var sender = new WebSocketSender(webSocket);
                var changeMessage = new WebSocketChangeRoom(message.UserId, transactionID, message.Action, fromChatRoomName, toChatRoomName, chatRoomContent);
                await sender.SendAsync(changeMessage, cancellationToken);

             
                //new message    
                var newMessage = new WebSocketBaseMessage
                {
                    UserId = fromChatRoomName,
                    TransactionId = Guid.NewGuid().ToString(),
                    Action = Name,
                    SubAction = "start",
                    Content = chatRoomContent,
                };

                await SendMessageToRoom(fromChatRoomName,newMessage, webSocket, speech);
            }
          
        }
 
    }
}
