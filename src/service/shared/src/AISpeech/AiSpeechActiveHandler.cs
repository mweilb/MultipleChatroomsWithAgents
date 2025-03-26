using Microsoft.SemanticKernel;
using MultiAgents.AzureAISpeech;
using MultiAgents.WebSockets;
using System.Net.WebSockets;


namespace multi_agents_shared.src.AISpeech
{
    public class AiSpeechActiveHandler
    {
        private readonly IAgentSpeech? _speachAgent = null;
        public AiSpeechActiveHandler(WebSocketHandler webSocketHandler, IAgentSpeech? iAgentSppech)
        {
            _speachAgent = iAgentSppech;
            webSocketHandler.RegisterCommand("voice", HandleSpeechState);
        }

        private Task HandleSpeechState(WebSocketBaseMessage message, WebSocket socket,Kernel _, IAgentSpeech __, ConnectionMode ___)
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
