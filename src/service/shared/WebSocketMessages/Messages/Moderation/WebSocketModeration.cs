namespace WebSocketMessages.Messages.Moderation
{
    public class WebSocketModeration : WebSocketBaseMessage
    {
        public string? Why { get; internal set; }
    }
}
