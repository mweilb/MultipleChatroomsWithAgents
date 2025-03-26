 
using MultiAgents.WebSockets;
 

namespace MultiAgents.AzureAISpeech.WebSockets
{
    /// <summary>
    /// Represents a WebSocket message containing audio data.
    /// Inherits common properties from WebSocketBaseMessage and adds audio-specific fields.
    /// </summary>
    public class WebSocketVoiceMessage : WebSocketBaseMessage
    {
        /// <summary>
        /// Gets or sets the binary audio content.
        /// This may be a complete audio file or a chunk of an audio stream.
        /// </summary>
        public byte[] AudioData { get; set; } = Array.Empty<byte>();

        /// <summary>
        /// Gets or sets the audio format (e.g., "pcm", "wav").
        /// </summary>
        public string AudioFormat { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the sample rate of the audio in Hertz.
        /// </summary>
        public int SampleRate { get; set; }
    }
}
