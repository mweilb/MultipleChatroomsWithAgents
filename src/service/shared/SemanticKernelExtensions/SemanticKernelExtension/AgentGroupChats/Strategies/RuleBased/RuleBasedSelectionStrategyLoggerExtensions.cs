using Microsoft.Extensions.Logging;
using System;

namespace SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased
{
    /// <summary>
    /// Provides extension methods for logging rule-based selection strategy events.
    /// Allows specifying the rule's name but removes all references to an "event".
    /// </summary>
    public static class RuleBasedSelectionStrategyLoggerExtensions
    {
        // Logs when an agent selection process starts.
        // Format placeholders: {Method}, {RuleName}, {StrategyType}
        private static readonly Action<ILogger, string, string, string, Exception?> s_logSelectingAgent =
            LoggerMessage.Define<string, string, string>(
                logLevel: LogLevel.Debug,
                eventId: new EventId(110, nameof(LogRuleBasedSelectingAgent)),
                formatString: "[{Method}] - Rule: {RuleName} - Selecting agent using strategy: {StrategyType}");

        // Logs when no agent could be selected.
        // Format placeholders: {Method}, {RuleName}
        private static readonly Action<ILogger, string, string, Exception?> s_logNoAgentSelected =
            LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Error,
                eventId: new EventId(111, nameof(LogRuleBasedNoAgentSelected)),
                formatString: "[{Method}] - Rule: {RuleName} - Unable to determine next agent.");

        // Logs when an agent is successfully selected.
        // Format placeholders: {Method}, {RuleName}, {AgentType}, {AgentId}, {AgentName}, {StrategyType}
        private static readonly Action<ILogger, string, string, string, string, string, string, Exception?> s_logSelectedAgent =
            LoggerMessage.Define<string, string, string, string, string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(112, nameof(LogRuleBasedSelectedAgent)),
                formatString: "[{Method}] - Rule: {RuleName} - Agent selected: {AgentType}: {AgentId}/{AgentName} using strategy: {StrategyType}");

        /// <summary>
        /// Logs that an agent selection process has started.
        /// </summary>
        /// <param name="logger">The logger instance to log the message.</param>
        /// <param name="method">The name of the invoking method (e.g. nameof(SelectAgentAsync)).</param>
        /// <param name="ruleName">The name of the rule being used.</param>
        /// <param name="strategyType">The type of the selection strategy.</param>
        public static void LogRuleBasedSelectingAgent(
            this ILogger logger,
            string method,
            string ruleName,
            Type strategyType)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                s_logSelectingAgent(
                    logger,
                    method,
                    ruleName,
                    strategyType.ToString(),
                    null);
            }
        }

        /// <summary>
        /// Logs that no agent could be selected.
        /// </summary>
        /// <param name="logger">The logger instance to log the message.</param>
        /// <param name="method">The name of the invoking method (e.g. nameof(SelectAgentAsync)).</param>
        /// <param name="ruleName">The name of the rule being used.</param>
        /// <param name="exception">The exception representing the failure.</param>
        public static void LogRuleBasedNoAgentSelected(
            this ILogger logger,
            string method,
            string ruleName,
            Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                s_logNoAgentSelected(
                    logger,
                    method,
                    ruleName,
                    exception);
            }
        }

        /// <summary>
        /// Logs that an agent was successfully selected.
        /// </summary>
        /// <param name="logger">The logger instance to log the message.</param>
        /// <param name="method">The name of the invoking method (e.g. nameof(SelectAgentAsync)).</param>
        /// <param name="ruleName">The name of the rule being used.</param>
        /// <param name="agentType">The type of the selected agent.</param>
        /// <param name="agentId">The identifier of the selected agent.</param>
        /// <param name="agentName">The display name of the selected agent.</param>
        /// <param name="strategyType">The type of the selection strategy.</param>
        public static void LogRuleBasedSelectedAgent(
            this ILogger logger,
            string method,
            string ruleName,
            Type agentType,
            string agentId,
            string agentName,
            Type strategyType)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                s_logSelectedAgent(
                    logger,
                    method,
                    ruleName,
                    agentType.ToString(),
                    agentId,
                    agentName,
                    strategyType.ToString(),
                    null);
            }
        }
    }
}
