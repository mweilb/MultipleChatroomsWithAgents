namespace WebSocketMessages.Messages.Librarians
{

    public class WebSocketLibrainDocRef
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
        public List<WebSocketLibrainDocRef> References = [];
      
    }
}
