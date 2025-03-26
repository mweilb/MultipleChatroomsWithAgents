 
using MultiAgents.AzureAISpeech;
using MultiAgents.WebSockets;
using System.Net.WebSockets;


namespace MultiAgents.AgentsChatRoom.Rooms
{
    public interface IMultiAgentHandler
    {
        /// <summary>
        /// Handles a command and returns the necessary data to process the chat room change.
        /// </summary>
        /// <param name="user">User identifier.</param>
        /// <param name="message">Incoming message details.</param>
        /// <param name="webSocket">The active WebSocket connection.</param>
        /// <param name="speech">Agent speech interface.</param>
        /// <returns>A tuple containing whether a new chat room was created, the chat room name, content, and a transaction ID.</returns>
        Task<(bool newChatRoom, string chatRoomName, string chatRoomContent, WebSocketBaseMessage)> HandleCommandAsync(
            string user,
            WebSocketBaseMessage message,
            WebSocket webSocket,
            ConnectionMode mode,
            IAgentSpeech speech);

    }
}
