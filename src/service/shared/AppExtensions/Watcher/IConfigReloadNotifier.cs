namespace AppExtensions.Watcher
{
    public interface IConfigReloadNotifier
    {
        void NotifyConfigReload(WebSocketMessages.Messages.WebSocketConfigReloadedMessage message);
    }
}
