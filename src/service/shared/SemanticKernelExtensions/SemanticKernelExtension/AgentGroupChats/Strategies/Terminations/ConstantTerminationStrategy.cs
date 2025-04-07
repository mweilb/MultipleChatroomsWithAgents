using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.Terminations
{

#pragma warning disable SKEXP0110
    /// <summary>
    /// A termination strategy that always returns a constant value based on constructor input.
    /// </summary>
    public sealed class ConstantTerminationStrategy : TerminationStrategy
    {
        private readonly bool _shouldTerminate;

        /// <summary>
        /// Agents that can cause termination (if null, all can).
        /// </summary>
        public IReadOnlyList<Agent>? AvailableAgents { get; set; } = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConstantTerminationStrategy"/> class.
        /// </summary>
        /// <param name="shouldTerminate">
        /// A boolean value that determines whether the termination should signal to stop (true) or continue (false).
        /// </param>
        /// <param name="agents">An optional list of agents that can cause termination. If null, all agents can terminate.</param>
        public ConstantTerminationStrategy(bool shouldTerminate, IReadOnlyList<Agent>? agents= null)
        {
            _shouldTerminate = shouldTerminate;
            Agents = agents;
        }

        /// <inheritdoc/>
        protected override Task<bool> ShouldAgentTerminateAsync(
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken = default)
        {
            if (AvailableAgents is null)
            {
                // If Agents is null, allow any agent to trigger termination.
                return Task.FromResult(_shouldTerminate);
            }
            else
            {
                // Otherwise, only agents in the Agents list can trigger termination.
                bool isAgentAllowed = AvailableAgents.Contains(agent);
                return Task.FromResult(isAgentAllowed && _shouldTerminate);
            }
        }
    }
}
