using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;



namespace AppExtensions.AISpeech
{
    public interface IAgentSpeech
    {
        bool Initialize(IConfiguration configuration);
        Task<bool> StreamTtsAudioAsync(WebSocket webSocket, string text);
        void SetActive(bool voiceOn);
    }

}
