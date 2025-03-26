
using MultiAgents.WebSockets;

namespace MultiAgents.AgentsChatRoom.WebSockets
{
    public class WebSocketModeration : WebSocketBaseMessage
    {
        public string? Why { get; internal set; }
    }
}
