using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
 

#pragma warning disable SKEXP0110
 

namespace MultiAgents.SemanticKernel.Modifications
{
    /// <summary>
    /// Abstract base class for selection strategies that determine which agent should respond next.
    /// This class extends the <see cref="SelectionStrategy"/> and is specifically designed for streaming scenarios.
    /// </summary>
    public abstract class SelectionStreamingStrategy : SelectionStrategy
    {
        /// <summary>
        /// Selects an agent for the next step in the streaming process based on the provided content and history.
        /// Derived classes should implement this method with the specific logic for selecting agents.
        /// </summary>
        /// <param name="content">The streaming content for the current message.</param>
        /// <param name="agents">A list of available agents that can respond.</param>
        /// <param name="history">The conversation history, including all previous messages.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous sequence of selected agents.</returns>
        public abstract IAsyncEnumerable<Agent?> SelectAgentStreaming(
            AgentStreamingContent content,
            IReadOnlyList<Agent> agents,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken = default);
    }
}

 
#pragma warning restore SKEXP0110
