

using AICreateAndIterate.FixErrors;
using AICreateAndIterate.FixErrors.Events;
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0080
namespace AICreateAndIterate
{
    public class ConsoleKernelProcessMessageChannel : IExternalKernelProcessMessageChannel
    {

        public YamlFixState? State { get; set; } = null;
        public string ChannelName => "ConsoleKernelProcessMessageChannel";
        public bool WaitingOnEvent = true;
        public string EventName { get; set; } = string.Empty;
        public ValueTask Initialize()
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask Uninitialize()
        {
            return ValueTask.CompletedTask;
        }

        public Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
        {
            EventName = externalTopicEvent;
            WaitingOnEvent = true;
            if (message.EventData != null)
            {
                State = message.EventData.ToObject() as YamlFixState;
            }
             
            return Task.CompletedTask;
        }
    }
}
