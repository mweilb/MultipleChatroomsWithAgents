using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Diagnostics;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Implements a rule-based selection strategy for choosing an agent based on predefined rules.
    /// This strategy uses injected settings and pre-defined constants to avoid hard-coded values.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RuleBasedSelectionStrategy"/> class with the specified settings.
    /// </remarks>
    /// <param name="settings">The rule-based settings to use. This should be injected.</param>
#pragma warning disable SKEXP0110
    public class RuleBasedSelectionStrategy(RuleBasedSettings settings) : SelectionStrategy
#pragma warning restore SKEXP0110
    {
        // Holds the settings used to determine which rule to apply.
        // This is provided via dependency injection.
        private readonly RuleBasedSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings), "Settings cannot be null.");

        // Constants for special rule designations to avoid hard-coding strings.
        private const string StartRuleDesignation = "start";
        private const string AnyRuleDesignation = "any";
        private const string UserDesignation = "user";

        /// <summary>
        /// Selects an agent based on the conversation history and the defined rule-based settings.
        /// Assumes that the underlying selection strategy (rule.Selection) returns an agent from the provided list.
        /// </summary>
        /// <param name="agents">The list of available agents.</param>
        /// <param name="history">The conversation history.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the selected agent.
        /// </returns>
        protected override async Task<Agent> SelectAgentAsync(
            IReadOnlyList<Agent> agents,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken = default)
        {
            // Validate the agents list.
            ArgumentNullException.ThrowIfNull(agents, "Agents list cannot be null.");

            // Early return if only one agent is available.
            if (agents.Count == 1)
            {
                return agents[0];
            }

            // Determine if this is the first time (i.e. no history exists).
            bool noHistory = !history.Any();
            string lastAuthorName = string.Empty;

            // If there is conversation history, get the last message's author name.
            if (!noHistory)
            {

#pragma warning disable SKEXP0001
                var lastEntry = history[history.Count - 1];
                ArgumentNullException.ThrowIfNull(lastEntry.AuthorName, "Last message AuthorName cannot be null.");
                lastAuthorName = lastEntry.AuthorName;
#pragma warning restore SKEXP0001
            }

            // If the last message was from "user", use the ContinuationAgentName name if available.
            if (lastAuthorName.Equals(UserDesignation, StringComparison.OrdinalIgnoreCase))
            {
                // Set the current rule in settings.
                if (_settings.CurrentRule != null)
                {
                    lastAuthorName = _settings.CurrentRule.ContinuationAgentName;
                }
            }

            // Retrieve the rule based on the last agent name and whether this is the first entry.
            var rule = GetRuleBasedOnCurrentAgent(lastAuthorName, noHistory);
            if (rule == null)
            {
                // Log error here if needed.
                throw new InvalidOperationException("No valid rule was found based on the provided agent name and history.");
            }

            // Set the current rule in settings.
            _settings.CurrentRule = rule; ;
            if (rule.Selection is null)
            {
                throw new InvalidOperationException("The selected rule does not have a valid selection strategy.");
            }

            // Log the chosen rule details here if desired.
            Debug.WriteLine($"Using rule with current agents: {string.Join(",", rule.CurrentAgentNames)}");

            // Use the rule's selection strategy to determine the next agent.
            // The NextAsync method is expected to return an agent from the provided list.
            var filteredAgents = (rule.NextAgentsNames is { Count: > 0 })
                ? [.. agents.Where(a => rule.NextAgentsNames.Contains(a.Name, StringComparer.OrdinalIgnoreCase))]
                : agents;
            
      
            Agent selectedAgent;
            if (filteredAgents.Count > 1)
            {
                selectedAgent = await rule.Selection.NextAsync(filteredAgents, history, cancellationToken)
                                                         .ConfigureAwait(false);
            }
            else
            {
                selectedAgent = filteredAgents[0];
            }


            if (selectedAgent is null)
            {
                // Log error here if needed.
                throw new InvalidOperationException("The selection strategy did not return a valid agent.");
            }

            return selectedAgent;
        }

        /// <summary>
        /// Retrieves the rule-based definition that matches the current agent's name.
        /// This method is marked private to encapsulate the rule selection logic.
        /// </summary>
        /// <param name="lastAgentName">The name of the last agent involved in the conversation.</param>
        /// <param name="noHistory">
        /// True if this is the first time entering the rules (i.e., no conversation history exists); otherwise, false.
        /// </param>
        /// <returns>
        /// The matching <see cref="RuleBasedDefinition"/>, or a fallback rule if no exact match is found.
        /// </returns>
        private RuleBasedDefinition? GetRuleBasedOnCurrentAgent(string lastAgentName, bool noHistory)
        {
            // Convert the IEnumerable to a list to avoid multiple enumerations and ensure immutability.
            var definitions = _settings.RuleBasedDefinitions?.ToList()
                              ?? throw new ArgumentNullException(nameof(_settings.RuleBasedDefinitions), "Rule definitions cannot be null.");

            // Early out if only one rule is available.
            if (definitions.Count == 1)
            {
                return definitions.First();
            }

            // If entering for the first time, look for a rule with a "start" designation.
            if (noHistory)
            {
                var ruleStart = definitions.FirstOrDefault(rule =>
                    rule.CurrentAgentNames.Any(c => c.Equals(StartRuleDesignation, StringComparison.OrdinalIgnoreCase)));
                if (ruleStart != null)
                {
                    return ruleStart;
                }
            }

            // Try to find a rule based on the last agent's name.
            var ruleBasedOnLastTermination = definitions.FirstOrDefault(rule =>
                rule.CurrentAgentNames.Any(c => c.Equals(lastAgentName, StringComparison.OrdinalIgnoreCase)));
            if (ruleBasedOnLastTermination != null)
            {
                return ruleBasedOnLastTermination;
            }

            // Fallback: look for a rule matching "any" or return the first rule.
            var ruleAny = definitions.FirstOrDefault(rule =>
                rule.CurrentAgentNames.Any(c => c.Equals(AnyRuleDesignation, StringComparison.OrdinalIgnoreCase)));

            return ruleAny ?? definitions.First();
        }
    }
}
