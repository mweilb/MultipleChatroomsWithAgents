 
using Microsoft.Extensions.Logging;
using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.AzureAISpeech;
using MultiAgents.SemanticKernel.Modifications;
using MultiAgents.WebSockets;
using System.Net.WebSockets;

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace MultiAgents.AgentsChatRoom.Rooms
{
    /// <summary>
    /// Defines the contract for a multi-agent chat room handler.
    /// Implementations are responsible for initializing agents, handling command invocations,
    /// and providing custom termination and selection strategies.
    /// </summary>
    public interface IMultiAgentChatRoom
    {   
        /// <summary>
        /// Gets the command name associated with this chat room.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the emoji representation for this chat room.
        /// </summary>
        string Emoji { get; }

        /// <summary>
        /// Initializes the agent(s) for the chat room handler.
        /// </summary>
        /// <param name="agentRegistry">The agent registry containing agent definitions.</param>
        /// <param name="chatRoom">The chat room instance to be configured.</param>
        /// <param name="logger">Logger for tracking initialization and runtime events.</param>
        void Initialize(AgentStreamingChatRoom chatRoom, ILogger<IMultiAgentChatRoom> logger);

 

        /// <summary>
        /// Handles an incoming command invocation from a WebSocket message.
        /// </summary>
        /// <param name="message">The incoming WebSocket message.</param>
        /// <param name="webSocket">The WebSocket connection for sending responses.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<(bool, string, string, WebSocketBaseMessage)> HandleCommandAsync(string author, WebSocketBaseMessage message, WebSocket webSocket, ConnectionMode mode, IAgentSpeech speech);

        /// <summary>
        /// Engages the moderator for reviewing and acting on flagged content from a user.
        /// This is typically used when a message or command needs moderation due to inappropriate content or behavior.
        /// </summary>
        /// <param name="sender">The WebSocket sender used to communicate with the moderator or other agents.</param>
        /// <param name="userId">The ID of the user whose content requires moderation.</param>
        /// <param name="command">The command that was issued, which may have triggered the moderation process.</param>
        /// <param name="transactionId">A unique ID for tracking the moderation process.</param>
        /// <param name="textToModerate">The actual content that is under review for moderation.</param>
        void EngageModerator(IWebSocketSender sender, string userId, string command, string transactionId, string textToModerate);


  }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0110
