
using MultiAgents.AzureAISpeech;
using MultiAgents.WebSockets;
using OllamaSharp;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
 

namespace MultiAgents.AgentsChatRoom.WebSockets
{
    /// <summary>
    /// Default implementation of <see cref="IWebSocketSender"/> that sends JSON messages over an actual WebSocket.
    /// </summary>
    public class WebSocketSender : IWebSocketSender
    {
        // The underlying WebSocket used for sending messages.
        private readonly WebSocket webSocket;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketSender"/> class.
        /// </summary>
        /// <param name="webSocket">The WebSocket instance to send messages through.</param>
        public WebSocketSender(WebSocket webSocket)
        {
            this.webSocket = webSocket;
        }



        /// <summary>
        /// Asynchronously sends a chat room reply message as a JSON string over the WebSocket.
        /// </summary>
        /// <param name="message">The chat room reply message to be sent.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public async Task SendAsync(WebSocketReplyChatRoomMessage message,ConnectionMode mode, CancellationToken cancellationToken = default)
        {
            // Configure JSON serialization options to format the output and include public fields.
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };

            if (mode == ConnectionMode.App)
            {
                //for app, if not content, nothing to send
                if (string.IsNullOrWhiteSpace(message.Content))
                {
                    return;
                }

                //no hints either
                message.Hints = [];

            }
    		
            // Serialize the message to a JSON string.
            string json = JsonSerializer.Serialize(message, options);
            // Convert the JSON string to UTF8-encoded bytes.
            var bytes = Encoding.UTF8.GetBytes(json);

            // Send the JSON message over the WebSocket.
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: cancellationToken
            );
        }

        public async Task SendAsync(WebSocketChangeRoom message, ConnectionMode mode, CancellationToken cancellationToken = default)
        {

            if (mode == ConnectionMode.App)
            {
                //no hints either
                message.Hints = [];

            }

            // Configure JSON serialization options to format the output and include public fields.
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true
            };
            //Console.WriteLine($"Voice message: {message.Content}");
            //AzureSpeech azureSpeech = new AzureSpeech();
            //await azureSpeech.StreamTtsAudioAsync(message.Content);

            // Serialize the message to a JSON string.
            string json = JsonSerializer.Serialize(message, options);
            // Convert the JSON string to UTF8-encoded bytes.
            var bytes = Encoding.UTF8.GetBytes(json);

            // Send the JSON message over the WebSocket.
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                endOfMessage: true,
                cancellationToken: cancellationToken
            );
        }

        public async Task SendError(string userId, string command, string who, string what)
        {
            var errorResponse = new WebSocketBaseMessage
            {
                Action = "error",
                SubAction = command,
                Content = who + " : " + what
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(errorResponse));
            await webSocket.SendAsync(new ArraySegment<byte>(errorJson), WebSocketMessageType.Text, true, CancellationToken.None);

        }

        public async Task SendModerationConcern(string userId, string command, string transactionId, string textToModerate, string why)
        {

            var moderationRequest = new WebSocketModeration
            {
                Action = "moderator",
                SubAction = command,
                TransactionId = transactionId,
                UserId = userId,
                Why = why,
                Content = textToModerate,
            };

            var errorJson = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(moderationRequest));
            await webSocket.SendAsync(new ArraySegment<byte>(errorJson), WebSocketMessageType.Text, true, CancellationToken.None);

        }

        public async Task SendSpeachAsync(WebSocketReplyChatRoomMessage currentMessage, IAgentSpeech speech, CancellationToken cancellationToken)
        {
            await speech.StreamTtsAudioAsync(webSocket, currentMessage.Content);
        }
    }
}
