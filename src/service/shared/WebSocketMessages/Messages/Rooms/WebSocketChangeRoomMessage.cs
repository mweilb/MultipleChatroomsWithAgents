 
namespace WebSocketMessages.Messages.Rooms
{
    public class WebSocketChangeRoom : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets the name of the agent that is responding.
        /// </summary>
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the emoji representing the actor.
        /// </summary>
        public string To { get; set; } = string.Empty;

        public WebSocketChangeRoom(string userID, string transactionID, string group, string from, string to, string content)
        {
            UserId = userID;
            TransactionId = transactionID;
            Action = group;
            SubAction = "change-room";
            From = from;
            To = to;
            Content = content;
        }
    }
}
