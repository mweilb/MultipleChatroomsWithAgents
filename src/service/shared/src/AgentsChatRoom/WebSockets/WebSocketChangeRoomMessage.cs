


using MultiAgents.WebSockets;

namespace MultiAgents.AgentsChatRoom.WebSockets
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
            this.UserId = userID;
            this.TransactionId = transactionID;
            this.Action = group;
            this.SubAction = "change-room";
            this.From = from;
            this.To = to;
            this.Content = content;
        }
    }
}
