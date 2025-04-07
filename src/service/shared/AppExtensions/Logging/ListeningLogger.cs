using System.Diagnostics;
using Microsoft.Extensions.Logging;
using AppExtensions.Logging.Aggregators;
using Microsoft.SemanticKernel;

namespace AppExtensions.Logging
{


    /// <summary>
    /// A logger that wraps an inner logger. It uses a dictionary mapping from key phrases to lifecycle event types,
    /// keeps track of a processing state for the instance, and also captures external messages via LearningInterceptor.
    /// </summary>
    public class ListeningLogger : ILogger
    {
        private readonly ILogger _innerLogger;

        /// <summary>
        /// Tracks the processing state of the logger.
        /// </summary>
        public enum ProcessingState
        {
            NotProcessing,
            Processing
        }


        /// <summary>
        /// Gets the current processing state of the logger instance.
        /// </summary>
        public ProcessingState CurrentState { get; private set; }

        // Instance event for lifecycle events (reserved for additional handlers).
        public event Action<AgentLifecycleEventData>? OnAgentLifecycleEvent;

        // Dictionary for exact lookup mapping.
        // Maps a log key to the (lifecycle event type, processing state) to use when matched.
        private static readonly Dictionary<string, (AgentLifecycleEventType EventType, ProcessingState ProcessingState)> _eventMapping =
            new Dictionary<string, (AgentLifecycleEventType, ProcessingState)>
            {
                { "LogRuleBasedSelectingAgent", (AgentLifecycleEventType.AgentSelectionStarted, ProcessingState.Processing) },
                { "LogRuleBasedSelectedAgent",   (AgentLifecycleEventType.AgentSelected,        ProcessingState.NotProcessing) },
                { "LogRuleBasedNoAgentSelected", (AgentLifecycleEventType.AgentNotSelected,     ProcessingState.NotProcessing) },
                { "LogRuleTerminationStrategyEvaluatingCriteria", (AgentLifecycleEventType.TerminationBegan,   ProcessingState.Processing) },
                { "LogRuleTerminationStrategyEvaluatedCriteria",  (AgentLifecycleEventType.TerminationEnded,   ProcessingState.NotProcessing) },
                { "LogRoomChangeStarted",        (AgentLifecycleEventType.RoomChangeStarted,     ProcessingState.Processing) },
                { "LogRoomChangeSelected",       (AgentLifecycleEventType.RoomChangeSelected,    ProcessingState.NotProcessing) },
                { "LogRoomChangeCanceled",       (AgentLifecycleEventType.RoomChangeCanceled,    ProcessingState.NotProcessing) }
            };

        // Dummy scope for when the inner logger does not supply one.
        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }

        /// <summary>
        /// Creates a new ListeningLogger wrapping the provided inner logger.
        /// Initializes the processing state to NotProcessing and subscribes to external message capture events.
        /// </summary>
        /// <param name="innerLogger">The inner logger to which log calls are forwarded.</param>
        public ListeningLogger(ILogger innerLogger)
        {
            _innerLogger = innerLogger;
            CurrentState = ProcessingState.NotProcessing;

            // Subscribe to external message capture events (from LearningInterceptor).
            LearningInterceptor.OnMessageCaptured += OnMessageCaptured;
            LearningInterceptor.OnMessageCapturedAsync += OnMessageCapturedAsync;
        }

        /// <summary>
        /// Explicit interface implementation for BeginScope.
        /// Returns a logging scope from the inner logger, or a dummy scope if the inner logger returns null.
        /// </summary>
        IDisposable ILogger.BeginScope<TState>(TState state) =>
            _innerLogger.BeginScope(state) ?? NullScope.Instance;

        /// <summary>
        /// Delegates the IsEnabled check to the inner logger.
        /// </summary>
        public bool IsEnabled(LogLevel logLevel) => _innerLogger.IsEnabled(logLevel);

        /// <summary>
        /// Logs a message using the inner logger.
        /// Uses a dictionary lookup based on EventId.Name:
        ///   - If a match is found, updates processing state accordingly.
        ///   - Otherwise, if still processing, raises an InfoBetweenEvents lifecycle event.
        /// Finally, forwards the log to the inner logger.
        /// </summary>
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);

            // Optional debug output.
            Debug.WriteLine(eventId.ToString());
            Debug.WriteLine(message);

            // Check for a matching event key using dictionary lookup.
            if (eventId.Name != null && _eventMapping.TryGetValue(eventId.Name, out var mapping))
            {
                // Key found: update processing state.
                CurrentState = mapping.ProcessingState;
            }
            else if (CurrentState == ProcessingState.Processing)
            {
                // No matching key, but we are still processing: raise an InfoBetweenEvents lifecycle event.
                RaiseLifecycleEvent(AgentLifecycleEventType.InfoBetweenEvents, message, exception);
            }

            // Forward the log message to the inner logger.
            _innerLogger.Log(logLevel, eventId, state, exception, formatter);
        }

        /// <summary>
        /// Raises a lifecycle event by generating a new unique identifier and
        /// emitting the event through both instance and global handlers.
        /// </summary>
        /// <param name="eventType">The type of lifecycle event.</param>
        /// <param name="message">The associated message.</param>
        /// <param name="exception">Optional exception.</param>
        private void RaiseLifecycleEvent(AgentLifecycleEventType eventType, string message, Exception? exception)
        {
            string id = Guid.NewGuid().ToString();
            EmitEvent(id, eventType, message, exception);
        }

        /// <summary>
        /// Creates a new lifecycle event data object and emits the event through instance and global handlers.
        /// </summary>
        /// <param name="id">The unique identifier for the event.</param>
        /// <param name="eventType">The lifecycle event type.</param>
        /// <param name="message">The message associated with the event.</param>
        /// <param name="exception">Optional exception details.</param>
        private void EmitEvent(string id, AgentLifecycleEventType eventType, string message, Exception? exception)
        {
            var eventData = new AgentLifecycleEventData(id, eventType, message, exception);
            OnAgentLifecycleEvent?.Invoke(eventData);
            AgentLifecycleEventAggregator.RaiseEvent(eventData);
        }

        /// <summary>
        /// Handles asynchronous external message capture events from LearningInterceptor.
        /// Converts the streaming chat message chunk to a string and raises an InfoBetweenEvents lifecycle event.
        /// </summary>
        /// <param name="eventName">An identifier for the event type.</param>
        /// <param name="id">A transaction identifier.</param>
        /// <param name="chunk">The streaming chat message content.</param>
        private async void OnMessageCapturedAsync(string eventName, string id, StreamingChatMessageContent chunk)
        {
            if (this.CurrentState == ProcessingState.Processing)
            {
                try
                {
                    StreamingChatMessageAggregator.RaiseEvent(new StreamingChatMessageEventData(eventName, id, chunk));

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error publishing streaming chat message event: {ex}");
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// Handles synchronous external message capture events from LearningInterceptor.
        /// Converts the chat message content to a string and raises an InfoBetweenEvents lifecycle event.
        /// </summary>
        /// <param name="eventName">An identifier for the event type.</param>
        /// <param name="message">The chat message content.</param>
        private void OnMessageCaptured(string eventName, ChatMessageContent message)
        {
            if (this.CurrentState == ProcessingState.Processing)
            {
                try
                {
                    // Generate a unique transaction ID for traceability.
                    ChatMessageAggregator.RaiseEvent(new ChatMessageEventData(eventName, message));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error publishing chat message event: {ex}");
                }
            }
        }
    }
}
