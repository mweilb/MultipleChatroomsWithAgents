using System.Collections.Generic;
using WebSocketMessages.Messages.Rooms;
using Xunit;

namespace WebSocketMessages.Tests
{
    public class WebSocketGetRoomsMessageTests
    {
        [Fact]
        public void Can_Create_And_Set_Rooms()
        {
            // Arrange
            var room = new WebSocketGetRooms
            {
                Name = "TestRoom",
                Emoji = "🏠"
            };
            var message = new WebSocketGetRoomsMessage();

            // Act
            message.Rooms.Add(room);

            // Assert
            Assert.Single(message.Rooms);
            Assert.Equal("TestRoom", message.Rooms[0].Name);
            Assert.Equal("🏠", message.Rooms[0].Emoji);
        }
    }
}
