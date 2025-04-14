 
namespace WebSocketMessages.Messages.Rooms
{
    public class WebSocketChangeRoom : WebSocketReplyChatRoomMessage
    {
        /// <summary>
        /// Gets or sets the name of the agent that is responding.
        /// </summary>
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string To { get; set; } = string.Empty;
 
    }
}
