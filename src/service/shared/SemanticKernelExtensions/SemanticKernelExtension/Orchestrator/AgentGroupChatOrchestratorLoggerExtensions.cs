using Microsoft.Extensions.Logging;

namespace SemanticKernelExtension.Orchestrator
{
    public static class AgentGroupChatOrchestratorLoggerExtensions
    {
        /// <summary>
        /// Logs that a room change has started.
        /// </summary>
        private static readonly Action<ILogger, string, string, string, string, Exception?> s_logRoomChangeStarted =
            LoggerMessage.Define<string, string, string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(20, "LogRoomChangeStarted"),   
                formatString: "[{Method}] Room change started: orchestrator '{OrchestratorName}', current room '{CurrentRoom}', new room '{NewRoom}'");

        /// <summary>
        /// Logs that a room change has been successfully applied.
        /// </summary>
        private static readonly Action<ILogger, string, string, string, Exception?> s_logRoomChangeSelected =
            LoggerMessage.Define<string, string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(21, "LogRoomChangeSelected"),
                formatString: "[{Method}] Room change selected: orchestrator '{OrchestratorName}' switched to new room '{NewRoom}'");

        /// <summary>
        /// Logs that a room change was canceled.
        /// </summary>
        private static readonly Action<ILogger, string, string, string, Exception?> s_logRoomChangeCanceled =
            LoggerMessage.Define<string, string, string>(
                logLevel: LogLevel.Warning,
                eventId: new EventId(22, "LogRoomChangeCanceled"),
                formatString: "[{Method}] Room change canceled: orchestrator '{OrchestratorName}', already in room '{Room}' or room not available");

        /// <summary>
        /// Logs that a room change has started.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="method">The method invoking this log (e.g. nameof(InvokeStreamingAsync)).</param>
        /// <param name="orchestratorName">The name of the orchestrator.</param>
        /// <param name="currentRoom">The current room name.</param>
        /// <param name="newRoom">The new room name.</param>
        public static void LogRoomChangeStarted(this ILogger logger, string method, string orchestratorName, string currentRoom, string newRoom)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                s_logRoomChangeStarted(logger, method, orchestratorName, currentRoom, newRoom, null);
            }
        }

        /// <summary>
        /// Logs that a room change was successfully selected.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="method">The method invoking this log.</param>
        /// <param name="orchestratorName">The name of the orchestrator.</param>
        /// <param name="newRoom">The new room name selected.</param>
        public static void LogRoomChangeSelected(this ILogger logger, string method, string orchestratorName, string newRoom)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                s_logRoomChangeSelected(logger, method, orchestratorName, newRoom, null);
            }
        }

        /// <summary>
        /// Logs that a room change was canceled.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="method">The method invoking this log.</param>
        /// <param name="orchestratorName">The name of the orchestrator.</param>
        /// <param name="room">The room that was attempted.</param>
        public static void LogRoomChangeCanceled(this ILogger logger, string method, string orchestratorName, string room)
        {
            if (logger.IsEnabled(LogLevel.Warning))
            {
                s_logRoomChangeCanceled(logger, method, orchestratorName, room, null);
            }
        }
    }
}
