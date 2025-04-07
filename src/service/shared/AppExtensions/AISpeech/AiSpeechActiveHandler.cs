using System.Net.WebSockets;
using WebSocketMessages;
using WebSocketMessages.Messages;


namespace AppExtensions.AISpeech
{
    public class AiSpeechActiveHandler
    {
        private readonly IAgentSpeech? _speachAgent = null;
        public AiSpeechActiveHandler(WebSocketHandler webSocketHandler, IAgentSpeech? iAgentSppech)
        {
            _speachAgent = iAgentSppech;
            webSocketHandler.RegisterCommand("voice", HandleSpeechStateAsync);
        }

        private Task HandleSpeechStateAsync(WebSocketBaseMessage message, WebSocket socket, ConnectionMode ___)
        {
            if (_speachAgent != null)
            {
                if (message.SubAction == "on")
                {
                    _speachAgent.SetActive(true);
                }
                else if (message.SubAction == "off")
                {
                    _speachAgent.SetActive(false);
                }
            }

            return Task.CompletedTask;
        }
    }
}
