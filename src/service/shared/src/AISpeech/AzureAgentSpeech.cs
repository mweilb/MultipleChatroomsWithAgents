using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using MultiAgents.AzureAISpeech.WebSockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
 

namespace MultiAgents.AzureAISpeech
{
    public class AzureAgentSpeech : IAgentSpeech
    {
        private SpeechConfig? _speechConfig = null;
        private bool _activeSpeech = false;

        public void SetActive(bool voiceOn)
        {
            Console.WriteLine($"[INFO] NullAgentSpeech: Voice Active = {voiceOn}");
            _activeSpeech = true;
        }

        public bool Initialize(IConfiguration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            var subscriptionKey = configuration["AzureSpeech:SubscriptionKey"];
            var region = configuration["AzureSpeech:Region"];

            if (string.IsNullOrEmpty(subscriptionKey) || string.IsNullOrEmpty(region))
                return false;

            _speechConfig = SpeechConfig.FromSubscription(subscriptionKey, region);
            return true;
        }

        public async Task<bool> StreamTtsAudioAsync(WebSocket webSocket, string text)
        {
            if (_activeSpeech == false)
            {
                Console.WriteLine("[Warning] Speach is not Active");
                return false;
            }

            if (webSocket == null || webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("[ERROR] WebSocket is not connected.");
                return false;
            }

            if (_speechConfig == null)
            {
                Console.WriteLine("[ERROR] Speech configuration is not initialized.");
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("[ERROR] Text to synthesize is empty.");
                return false;
            }

            Console.WriteLine("[INFO] Creating speech synthesizer...");
            using var synthesizer = new SpeechSynthesizer(_speechConfig, audioConfig: null);

            Console.WriteLine("[INFO] Starting speech synthesis...");
            var result = await synthesizer.StartSpeakingTextAsync(text);
            using var audioDataStream = AudioDataStream.FromResult(result);

            Console.WriteLine("[INFO] Streaming audio data over WebSocket...");
            byte[] buffer = new byte[16000];
            uint bytesRead = 0;

            while ((bytesRead = audioDataStream.ReadData(buffer)) > 0)
            {
                // Copy only the bytes read
                byte[] chunk = new byte[bytesRead];
                Array.Copy(buffer, chunk, bytesRead);

                // Create a WebSocketAudioMessage packet for this audio chunk.
                var audioMessage = new WebSocketVoiceMessage
                {
                    UserId = "User123", // Replace with your actual user identifier if available
                    TransactionId = Guid.NewGuid().ToString(),
                    Action = "audio",
                    SubAction = "chunk",
                    AudioData = chunk,
                    AudioFormat = "pcm",   // Change if using a different audio format
                    SampleRate = 16000     // Adjust based on your audio configuration
                };

                // Serialize the packet to JSON. Note that the AudioData byte[] will be Base64-encoded.
                string jsonPayload = JsonSerializer.Serialize(audioMessage);
                byte[] messageBytes = Encoding.UTF8.GetBytes(jsonPayload);

                Console.WriteLine($"[INFO] Sending audio chunk with {messageBytes.Length} bytes...");
                // Send as a text message so that the JSON is preserved.
                await webSocket.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            // Send an end-of-stream message.
            var endMessage = new WebSocketVoiceMessage
            {
                UserId = "User123",
                TransactionId = Guid.NewGuid().ToString(),
                Action = "audio",
                SubAction = "done",
                AudioData = Array.Empty<byte>(),
                AudioFormat = "pcm",
                SampleRate = 16000
            };

            string endJson = JsonSerializer.Serialize(endMessage);
            byte[] endBytes = Encoding.UTF8.GetBytes(endJson);
            Console.WriteLine("[INFO] Sending end-of-stream audio message...");
            await webSocket.SendAsync(new ArraySegment<byte>(endBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            return true;
        }
    }
}
