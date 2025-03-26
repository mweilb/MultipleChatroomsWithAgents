 
using MultiAgents.WebSockets;

namespace MultiAgents.AgentsChatRoom.WebSockets
{
    /// <summary>
    /// Represents a reply message for a chat room sent over WebSocket.
    /// Contains additional hints for processing and the name of the agent responding.
    /// </summary>
    public class WebSocketReplyChatRoomMessage : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets a dictionary of additional hints for processing the message.
        /// These hints may include metadata such as processing status or debugging information.
        /// </summary>
        public Dictionary<string, object> Hints { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets or sets the name of the agent that is responding.
        /// </summary>
        public string AgentName { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public string SubRoomName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;
        public bool UserTurn { get; internal set; } = false;
    }
}
