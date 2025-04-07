

using Microsoft.SemanticKernel;

namespace AppExtensions.Logging.Aggregators
{

    /// <summary>
    /// Event data for streaming chat messages.
    /// </summary>
    public class StreamingChatMessageEventData(string eventName, string transactionId, StreamingChatMessageContent content)
    {
        public string EventName { get; } = eventName;
        public string TransactionId { get; } = transactionId;
        public StreamingChatMessageContent Content { get; } = content;
    }

    /// <summary>
    /// Global aggregator for streaming chat messages.
    /// </summary>
    public static class StreamingChatMessageAggregator
    {
        public static event Action<StreamingChatMessageEventData>? OnStreamingChatMessageEvent;
        public static void RaiseEvent(StreamingChatMessageEventData eventData) => OnStreamingChatMessageEvent?.Invoke(eventData);
    }
}
