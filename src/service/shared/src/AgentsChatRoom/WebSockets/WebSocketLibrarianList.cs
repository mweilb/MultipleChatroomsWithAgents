using MultiAgents.WebSockets;
 

namespace MultiAgents.AgentsChatRoom.WebSockets
{

    public class WebSocektLibrainDocRef
    {
        public string? DocumentUri;
        public string? Text ;
        public string? Question;
        public string? Score;
    }

    public class WebSocketLibrarianList : WebSocketBaseMessage
    {
        public string? Question;
        public string? RoomName;
        public string? AgentName;
        public List<WebSocektLibrainDocRef> References = [];
      
    }
}
