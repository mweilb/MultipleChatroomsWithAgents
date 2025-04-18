 
namespace WebSocketMessages.Messages.Rooms
{
    /// <summary>
    /// Represents a reply message for a chat room sent over WebSocket.
 /// </summary>
    public class WebSocketReplyChatRoomMessage : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets the name of the agent that is responding.
        /// </summary>
        public string AgentName { get; set; } = string.Empty;

        public string Orchestrator { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;
 
    }
}
