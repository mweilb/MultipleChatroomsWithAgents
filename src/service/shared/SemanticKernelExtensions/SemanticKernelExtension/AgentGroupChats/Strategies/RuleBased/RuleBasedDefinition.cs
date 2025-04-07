using Microsoft.SemanticKernel.Agents.Chat;


namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Defines the rule-based configuration for an agent's strategy.
    /// This includes the list of current agents, the list of next agents,
    /// and optional selection and termination strategies.
    /// </summary>
    public class RuleBasedDefinition
    {
        /// <summary>
        /// List of names for agents that are currently active or involved.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of names for agents that are currently active or involved.
        /// </summary>
        public List<string> CurrentAgentNames { get; set; } = [];

        /// <summary>
        /// List of names for agents that are scheduled to act next.
        /// </summary>
        public List<string> NextAgentsNames { get; set; } = [];

#pragma warning disable SKEXP0110
        /// <summary>
        /// Optional selection strategy used to determine which agent to select next.
        /// </summary>
        public SelectionStrategy? Selection { get; set; } = null;

        /// <summary>
        /// Optional termination strategy used to decide when to stop the process.
        /// </summary>
        public TerminationStrategy? Termination { get; set; } = null;
#pragma warning restore SKEXP0110

        /// <summary>
        /// The name of the agent that will continue the conversation after termination.
        /// </summary>
        public string ContinuationAgentName { get; set; } = string.Empty;
    }
}
