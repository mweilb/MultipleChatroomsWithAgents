using WebSocketMessages.Messages;
 

namespace WebSocketMessages.Messages.Rooms
{

    public class WebSocketRoomProfile
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of agents representing the chat room.
        /// </summary>
        public List<WebSocketAgentProfile> Agents { get; set; } = [];
    }

    /// <summary>
    /// Represents an actor (or agent) in a chat room with a name and an emoji.
    /// </summary>
    public class WebSocketAgentProfile
    {
        /// <summary>
        /// Gets or sets the name of the agent.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;
    }

    /// <summary>
    /// Represents a validation error for WebSocket messages.
    /// </summary>
    public class WebSocketValidationError
    {
        public string Message { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
        public int? CharPosition { get; set; }
    }

    /// <summary>
    /// Represents a WebSocket message that contains information about a specific chat room,
    /// including its associated room and actor details.
    /// </summary>
    public class WebSocketGetRooms : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets the name of the chat room.
        /// </summary>
        public string Name { get; set; } = string.Empty;

         /// <summary>
        /// Gets or sets the emoji representing the room.
        /// </summary>
        public string Emoji { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the auto start setting for the room.
        /// </summary>
        public string AutoStart { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MerMaid representing the room.
        /// </summary>
        public string MerMaidGraph { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the MerMaid representing the room.
        /// </summary>
        public string Yaml { get; set; } = string.Empty;

        public List<WebSocketValidationError> Errors { get; set; } = [];

        public List<WebSocketRoomProfile> Rooms { get; set; } = [];

       
    }


    /// <summary>
    /// Represents a WebSocket message used to send a list of available chat rooms.
    /// </summary>
    public class WebSocketGetRoomsMessage : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets the list of chat room messages.
        /// </summary>
        public List<WebSocketGetRooms> Rooms { get; set; } = [];
    }
}
