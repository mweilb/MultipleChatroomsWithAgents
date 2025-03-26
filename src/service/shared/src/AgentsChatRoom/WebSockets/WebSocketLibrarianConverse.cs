using MultiAgents.WebSockets;
 

namespace MultiAgents.AgentsChatRoom.WebSockets
{
    public class WebSocketLibrarianConverse : WebSocketBaseMessage
    {
        public string? Question;
        public string? Thinking;
        public string? RoomName;
        public string? AgentName;
    }
}
