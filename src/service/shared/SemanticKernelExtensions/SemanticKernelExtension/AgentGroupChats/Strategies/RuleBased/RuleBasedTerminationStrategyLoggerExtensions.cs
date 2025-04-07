using System;
using Microsoft.Extensions.Logging;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Provides extension methods for logging rule-based termination strategy events.
    /// </summary>
    public static class RuleBasedTerminationStrategyLoggerExtensions
    {
        // This delegate logs when the termination strategy is evaluating an agent.
        // The log message includes the method name, agent type, agent id, and agent display name.
        private static readonly Action<ILogger, string, string, string, string, Exception?> s_logTermStrategyEvaluatingCriteria =
            LoggerMessage.Define<string, string, string, string>(
                logLevel: LogLevel.Debug,
                eventId: new EventId(101, "LogRuleTerminationStrategyEvaluatingCriteria"),  
                formatString: "[{Method}] Evaluating termination criteria for agent {AgentType}: {AgentId}/{AgentName}");

        // This delegate logs the outcome after evaluating the termination strategy.
        // In addition to the data above, it includes a boolean indicating whether termination is required.
        private static readonly Action<ILogger, string, string, string, string, bool, Exception?> s_logTermStrategyEvaluatedCriteria =
            LoggerMessage.Define<string, string, string, string, bool>(
                logLevel: LogLevel.Debug,
                eventId: new EventId(101, "LogRuleTerminationStrategyEvaluatedCriteria"),
                formatString: "[{Method}] Evaluated termination criteria for agent {AgentType}: {AgentId}/{AgentName}. Terminate: {ShouldTerminate}");

        /// <summary>
        /// Logs that the termination strategy is evaluating the agent for termination.
        /// </summary>
        /// <param name="logger">The logger instance to log the message.</param>
        /// <param name="method">The name of the method invoking the log (e.g. nameof(ShouldTerminateAsync)).</param>
        /// <param name="agentType">The type of the agent under evaluation.</param>
        /// <param name="agentId">The identifier of the agent.</param>
        /// <param name="agentName">The display name of the agent.</param>
        public static void LogRuleTerminationStrategyEvaluatingCriteria(
            this ILogger logger,
            string method,
            Type agentType,
            string agentId,
            string agentName)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                s_logTermStrategyEvaluatingCriteria(logger, method, agentType.ToString(), agentId, agentName, null);
            }
        }

        /// <summary>
        /// Logs that the termination strategy has evaluated the agent, including the decision.
        /// </summary>
        /// <param name="logger">The logger instance to log the message.</param>
        /// <param name="method">The name of the method invoking the log (e.g. nameof(ShouldTerminateAsync)).</param>
        /// <param name="agentType">The type of the agent under evaluation.</param>
        /// <param name="agentId">The identifier of the agent.</param>
        /// <param name="agentName">The display name of the agent.</param>
        /// <param name="shouldTerminate">True if the agent should terminate; otherwise, false.</param>
        public static void LogRuleTerminationStrategyEvaluatedCriteria(
            this ILogger logger,
            string method,
            Type agentType,
            string agentId,
            string agentName,
            bool shouldTerminate)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                s_logTermStrategyEvaluatedCriteria(logger, method, agentType.ToString(), agentId, agentName, shouldTerminate, null);
            }
        }
    }
}
