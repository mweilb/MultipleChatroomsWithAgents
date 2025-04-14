using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Agents.Chat;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Represents the rule-based settings for an agent group chat.
    /// </summary>
    public class RuleBasedSettings :
#pragma warning disable SKEXP0110
        AgentGroupChatSettings
#pragma warning restore SKEXP0110
    {
        /// <summary>
        /// Gets or sets the collection of rule-based definitions.
        /// </summary>
        public IEnumerable<RuleBasedDefinition> RuleBasedDefinitions { get; set; } = [];

        /// <summary>
        /// Gets or sets the current active rule.
        /// </summary>
        public RuleBasedDefinition? CurrentRule { get; set; }= null;

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleBasedSettings"/> class with the specified rule-based definitions.
        /// This constructor also initializes the selection and termination strategies based on these settings.
        /// </summary>
        /// <param name="ruleBasedDefinitions">
        /// A collection of <see cref="RuleBasedDefinition"/> objects that define the rules for agent selection and termination.
        /// </param>
        public RuleBasedSettings(IEnumerable<RuleBasedDefinition> ruleBasedDefinitions, ILoggerFactory factory)
        {
            // Initialize the rule-based definitions from the provided collection.
            RuleBasedDefinitions = ruleBasedDefinitions;

            // Initialize the selection strategy using the current settings.
            SelectionStrategy = new RuleBasedSelectionStrategy(this, factory.CreateLogger<RuleBasedSelectionStrategy>());
 

            // Initialize the termination strategy using the current settings.
            TerminationStrategy = new RuleBasedTerminationStrategy(this, factory.CreateLogger<RuleBasedSelectionStrategy>());
        }
 

    }
}
