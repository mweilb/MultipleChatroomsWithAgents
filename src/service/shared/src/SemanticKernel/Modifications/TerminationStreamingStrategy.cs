using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
 

#pragma warning disable SKEXP0110
 

namespace MultiAgents.SemanticKernel.Modifications
{
    /// <summary>
    /// Abstract base class for termination strategies that determine whether an agent should stop responding.
    /// This class extends <see cref="TerminationStrategy"/> and is specifically designed for streaming scenarios.
    /// </summary>
    public abstract class TerminationStreamingStrategy : TerminationStrategy
    {

        public TerminationStreamingStrategy(string? name)
        {
            if (name != null && !string.IsNullOrEmpty(name)) {
                Name = name;
            }
        }

        public string Name = ""; 
        /// <summary>
        /// Determines whether an agent should terminate its response stream based on the provided content, agent, and conversation history.
        /// Derived classes should implement this method with specific termination logic.
        /// </summary>
        /// <param name="content">The current streaming content of the agent's message.</param>
        /// <param name="agent">The agent that is being evaluated for termination.</param>
        /// <param name="history">The conversation history, including previous messages exchanged.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of boolean values indicating whether the agent should terminate its response stream.</returns>
        public abstract IAsyncEnumerable<bool> ShouldAgentTerminateStreaming(
            AgentStreamingContent content,
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken);
    }
}
 
#pragma warning restore SKEXP0110
