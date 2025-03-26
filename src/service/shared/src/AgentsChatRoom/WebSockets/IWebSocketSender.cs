

using MultiAgents.AzureAISpeech;

namespace MultiAgents.AgentsChatRoom.WebSockets
{
    /// <summary>
    /// Provides an abstraction for sending WebSocket messages,
    /// enabling easier testing and mocking of WebSocket operations.
    /// </summary>
    public interface IWebSocketSender
    {
        /// <summary>
        /// Asynchronously sends a chat room reply message over a WebSocket connection.
        /// </summary>
        /// <param name="replyMessage">The chat room reply message to send.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        Task SendAsync(WebSocketReplyChatRoomMessage replyMessage, CancellationToken cancellationToken = default);
        Task SendSpeachAsync(WebSocketReplyChatRoomMessage currentMessage, IAgentSpeech speech, CancellationToken cancellationToken);

        Task SendModerationConcern(string userId, string command, string transactionId, string textToModerate, string why);
        Task SendError(string userId, string command, string who, string what);

    }
}
