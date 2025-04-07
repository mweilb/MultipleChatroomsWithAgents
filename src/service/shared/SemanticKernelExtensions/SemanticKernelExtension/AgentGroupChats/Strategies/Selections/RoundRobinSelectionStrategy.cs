using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
 

namespace SemanticKernelExtension.AgentGroupChats.Strategies.Selections
{
#pragma warning disable SKEXP0110
    public class RoundRobinSelectionStrategy(IEnumerable<Agent> agents) : SelectionStrategy
#pragma warning restore SKEXP0110
    {
        // Store the provided agents in an internal list.
        private readonly List<Agent> _agents = agents.ToList();

        // Keeps track of the current position in the round-robin rotation.
        private int _currentIndex = 0;

        /// <summary>
        /// Selects an agent using round-robin rotation.
        /// Starts with the first agent and picks the next agent in the list, wrapping around at the end.
        /// </summary>
        /// <param name="agents">
        /// The list of available agents.
        /// This parameter is not used since the internal list (_agents) is used for selection.
        /// </param>
        /// <param name="history">The conversation history (unused in this strategy).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The next agent in the round-robin order.</returns>
        protected override Task<Agent> SelectAgentAsync(
            IReadOnlyList<Agent> agents,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken = default)
        {
            if (_agents.Count == 0)
            {
                throw new InvalidOperationException("No agents available for selection.");
            }

            // Retrieve the current agent.
            Agent selectedAgent = _agents[_currentIndex];

            // Update the index to the next agent, wrapping around if necessary.
            _currentIndex = (_currentIndex + 1) % _agents.Count;

            return Task.FromResult(selectedAgent);
        }
    }
}
