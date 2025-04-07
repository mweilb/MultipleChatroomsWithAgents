using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppExtensions.Logging.Aggregators
{
    /// <summary>
    /// Event data for regular (non-streaming) chat messages.
    /// </summary>
    public class ChatMessageEventData
    {
        public string EventName { get; }
        public ChatMessageContent Content { get; }

        public ChatMessageEventData(string eventName,  ChatMessageContent content)
        {
            EventName = eventName;
            Content = content;
        }
    }

    /// <summary>
    /// Global aggregator for regular chat messages.
    /// </summary>
    public static class ChatMessageAggregator
    {
        public static event Action<ChatMessageEventData>? OnChatMessageEvent;
        public static void RaiseEvent(ChatMessageEventData eventData) => OnChatMessageEvent?.Invoke(eventData);
    }

}
