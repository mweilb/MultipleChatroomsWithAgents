using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;

namespace MultiAgents.AzureAISpeech
{
    public class NullAgentSpeech : IAgentSpeech
    {
        public bool Initialize(IConfiguration configuration)
        {
            // No initialization needed for the null implementation.
            Console.WriteLine("[INFO] NullAgentSpeech: Initialize called. No operation performed.");
            return true;
        }

        public Task<bool> StreamTtsAudioAsync(WebSocket webSocket, string text)
        {
            // Simply log that no work is done and return a completed task.
            Console.WriteLine("[INFO] NullAgentSpeech: StreamTtsAudioAsync called. No operation performed.");
            return Task.FromResult(true);
        }

        public void SetActive(bool voiceOn)
        {
            Console.WriteLine($"[INFO] NullAgentSpeech: Voice Active = {voiceOn}");
        }
    }
}
