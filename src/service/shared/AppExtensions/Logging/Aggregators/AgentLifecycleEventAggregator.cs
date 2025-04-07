namespace AppExtensions.Logging.Aggregators
{

    public enum AgentLifecycleEventType
    {
        AgentSelectionStarted,
        AgentSelected,
        AgentNotSelected,
        TerminationBegan,
        TerminationEnded,
        RoomChangeStarted,
        RoomChangeSelected,
        RoomChangeCanceled,
        InfoBetweenEvents,
    }

    /// <summary>
    /// Carries data for a lifecycle event.
    /// </summary>
    public class AgentLifecycleEventData
    {
        public string Id { get; } // Unique identifier for this event.
        public AgentLifecycleEventType EventType { get; }
        public string Message { get; }
        public Exception? Exception { get; }

        public AgentLifecycleEventData(string id, AgentLifecycleEventType eventType, string message, Exception? exception = null)
        {
            Id = id;
            EventType = eventType;
            Message = message;
            Exception = exception;
        }
    }

    /// <summary>
    /// Global aggregator for agent lifecycle events.
    /// Other components can subscribe to this aggregator to receive lifecycle event notifications.
    /// </summary>
    public static class AgentLifecycleEventAggregator
    {
        /// <summary>
        /// Occurs when an agent lifecycle event is raised.
        /// </summary>
        public static event Action<AgentLifecycleEventData>? OnAgentLifecycleEvent;

        /// <summary>
        /// Raises a lifecycle event.
        /// </summary>
        /// <param name="eventData">The lifecycle event data.</param>
        public static void RaiseEvent(AgentLifecycleEventData eventData)
        {
            OnAgentLifecycleEvent?.Invoke(eventData);
        }
    }

  

}