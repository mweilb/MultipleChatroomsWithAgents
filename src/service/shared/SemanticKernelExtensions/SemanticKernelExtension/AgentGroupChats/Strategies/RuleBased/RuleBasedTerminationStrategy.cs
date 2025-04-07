using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.Agents.Extensions;
using SemanticKernelExtension.Hacks;
using System.Data;


namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Implements a rule-based termination strategy that determines whether an agent should terminate.
    /// If termination is required, it updates the shared settings with the name of the continue agent.
    /// </summary>
#pragma warning disable SKEXP0110
    internal class RuleBasedTerminationStrategy(RuleBasedSettings settings,ILogger logger) : TerminationStrategy
#pragma warning restore SKEXP0110
    {
        // The shared settings that store rule-based definitions.
        // Provided via dependency injection.
        private readonly RuleBasedSettings _settings = settings ?? throw new ArgumentNullException(nameof(settings), "Settings cannot be null.");

        // Hide the base Logger with a new property.
        protected new ILogger Logger { get; } = logger;

        /// <summary>
        /// Asynchronously determines whether the specified agent should terminate based on the current rule's termination strategy.
        /// If termination is indicated, updates the settings with the name of the continue agent.
        /// </summary>
        /// <param name="agent">The agent to evaluate for termination.</param>
        /// <param name="history">The conversation history.</param>
        /// <param name="cancellationToken">A cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is a boolean indicating whether the agent should terminate.
        /// </returns>
        protected override async Task<bool> ShouldAgentTerminateAsync(
            Agent agent,
            IReadOnlyList<ChatMessageContent> history,
            CancellationToken cancellationToken)
        {
            // Retrieve the current rule that was previously set by the selection strategy.  
            ArgumentNullException.ThrowIfNull(_settings.CurrentRule);
            ArgumentNullException.ThrowIfNull(_settings.CurrentRule.Termination);


            this.Logger.LogRuleTerminationStrategyEvaluatingCriteria(nameof(ShouldTerminateAsync), agent.GetType(), agent.Id, agent.GetDisplayName());

            Logger.LogInformation($"Termination Evaluation base on rule: '{_settings.CurrentRule.Name}'");


            // Delegate the termination decision to the rule's termination strategy
            // and await the result before returning.
            var shouldTerminate = await _settings.CurrentRule.Termination.ShouldTerminateAsync(agent, history, cancellationToken)
                                          .ConfigureAwait(false);

            // Log the chosen rule details.
            Logger.LogInformation($"Should Terminate: {(shouldTerminate ? "Yes":"No")}");



            this.Logger.LogRuleTerminationStrategyEvaluatedCriteria(nameof(ShouldTerminateAsync), agent.GetType(), agent.Id, agent.GetDisplayName(), shouldTerminate);


            return shouldTerminate;
            
        }
    }
}
