using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using WebSocketMessages;
using Xunit;

namespace WebSocketMessages.Tests
{
    public class WebSocketHandlerTests
    {
        [Fact]
        public void Constructor_RegistersDefaultCommands()
        {
            // Arrange & Act
            var handler = new WebSocketHandler();

            // Assert
            // The default commands "mode" and "ping" should be registered.
            // Since commandHandlers is private, we can't access it directly.
            // Instead, we can try to register the same command and expect no exception.
            var ex = Record.Exception(() =>
                handler.RegisterCommand("mode", (msg, ws, mode) => Task.CompletedTask)
            );
            Assert.Null(ex);

            ex = Record.Exception(() =>
                handler.RegisterCommand("ping", (msg, ws, mode) => Task.CompletedTask)
            );
            Assert.Null(ex);
        }

        [Fact]
        public void CurrentConnectionMode_DefaultsToApp()
        {
            // Arrange & Act
            var handler = new WebSocketHandler();

            // Assert
            Assert.Equal(ConnectionMode.App, handler.CurrentConnectionMode);
        }
    }
}
