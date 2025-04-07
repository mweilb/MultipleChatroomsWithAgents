using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.Extensions; // For GetDisplayName() extension
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.Hacks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Implements a rule-based selection strategy for choosing an agent based on predefined rules.
    /// This strategy uses injected settings and pre-defined designations to avoid hard-coded values.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RuleBasedSelectionStrategy"/> class with the specified settings.
    /// </remarks>
    /// <param name="settings">The rule-based settings to use. This should be injected.</param>
#pragma warning disable SKEXP0110
    public class RuleBasedSelectionStrategy(RuleBasedSettings settings, ILogger logger) : SelectionStrategy
#pragma warning restore SKEXP0110
    {
        // Holds the settings used to determine which rule to apply.
        private readonly RuleBasedSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings), "Settings cannot be null.");

        // Constants for special rule designations.
        private const string StartRuleDesignation = "start";
        private const string AnyRuleDesignation = "any";
        private const string UserDesignation = "user";

        protected new ILogger Logger { get; } = logger?? NullLogger.Instance;



        /// <summary>
        /// Selects an agent based on the conversation history and the defined rule-based settings.
        /// This method uses the current rule and the rule's selection strategy to pick an agent.
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
            // Validate that the agents list is not null.
            ArgumentNullException.ThrowIfNull(agents, "Agents list cannot be null.");

            // If there's only one agent, return it immediately.
            if (agents.Count == 1)
            {
                return agents[0];
            }

            // Determine if there is no conversation history.
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

  
            // If the last message came from the user, override with the continuation agent name if set.
            if (lastAuthorName.Equals(UserDesignation, StringComparison.OrdinalIgnoreCase))
            {
                if (_settings.CurrentRule != null)
                {
                    lastAuthorName = _settings.CurrentRule.ContinuationAgentName;
                }
            }

            // Retrieve the rule based on the last agent name and the history flag.
            var rule = GetRuleBasedOnCurrentAgent(lastAuthorName, noHistory);
            if (rule == null)
            {
                throw new InvalidOperationException("No valid rule was found based on the provided agent name and history.");
            }

     
            // Set the current rule in settings.
            _settings.CurrentRule = rule;
            if (rule.Selection is null)
            {
                throw new InvalidOperationException("The selected rule does not have a valid selection strategy.");
            }

            // Log that the selection process is starting using the custom extension.
            // Here, we pass the rule's name as the event name.
            Logger.LogRuleBasedSelectingAgent(
                nameof(SelectAgentAsync),
                rule.Name,
                typeof(RuleBasedSelectionStrategy));

            // Log conversation context details.
            Logger.LogInformation($"Rule Selected - '{rule.Name}': Using Current Agent {{{lastAuthorName}}} and Next Agents [{string.Join(",", rule.NextAgentsNames)}]  ");



            // Filter agents based on NextAgentsNames if available, otherwise use all agents.
            var filteredAgents = (rule.NextAgentsNames is { Count: > 0 })
                ? agents.Where(a => rule.NextAgentsNames.Contains(a.Name, StringComparer.OrdinalIgnoreCase)).ToList()
                : agents.ToList();

            Agent selectedAgent;
            // Use the rule's selection strategy if more than one candidate remains.
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
                // Log the failure to select an agent.
                Logger.LogRuleBasedNoAgentSelected(
                    nameof(SelectAgentAsync),
                    rule.Name,
                    new Exception("No agent selected by strategy."));
                throw new InvalidOperationException("The selection strategy did not return a valid agent.");
            }
            else
            {
                // Log the success of agent selection.
                Logger.LogRuleBasedSelectedAgent(
                    nameof(SelectAgentAsync),
                    rule.Name,
                    selectedAgent.GetType(),
                    selectedAgent.Id,
                    selectedAgent.GetDisplayName(),
                    typeof(RuleBasedSelectionStrategy));
            }

            return selectedAgent;
        }

        /// <summary>
        /// Retrieves the rule-based definition that matches the current agent's name.
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
            // Convert rule definitions to a list to avoid multiple enumerations.
            var definitions = _settings.RuleBasedDefinitions?.ToList()
                              ?? throw new ArgumentNullException(nameof(_settings.RuleBasedDefinitions), "Rule definitions cannot be null.");

            // If only one rule is available, return it immediately.
            if (definitions.Count == 1)
            {
                return definitions.First();
            }

            // If there is no conversation history, look for a rule with a "start" designation.
            if (noHistory)
            {
                var ruleStart = definitions.FirstOrDefault(rule =>
                    rule.CurrentAgentNames.Any(c => c.Equals(StartRuleDesignation, StringComparison.OrdinalIgnoreCase)));
                if (ruleStart != null)
                {
                    return ruleStart;
                }
            }

            // Try to match a rule based on the last agent's name.
            var ruleBasedOnLastTermination = definitions.FirstOrDefault(rule =>
                rule.CurrentAgentNames.Any(c => c.Equals(lastAgentName, StringComparison.OrdinalIgnoreCase)));
            if (ruleBasedOnLastTermination != null)
            {
                return ruleBasedOnLastTermination;
            }

            // Fallback: Look for a rule matching "any" or return the first rule.
            var ruleAny = definitions.FirstOrDefault(rule =>
                rule.CurrentAgentNames.Any(c => c.Equals(AnyRuleDesignation, StringComparison.OrdinalIgnoreCase)));

            return ruleAny ?? definitions.First();
        }
    }
}
