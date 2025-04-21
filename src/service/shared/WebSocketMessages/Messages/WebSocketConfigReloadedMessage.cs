using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace WebSocketMessages.Messages
{
    public class WebSocketConfigReloadedMessage : WebSocketBaseMessage
    {
        [JsonPropertyName("ChangedRoom")]
        public Rooms.WebSocketGetRooms? ChangedRoom { get; set; } = null;
    }
}
