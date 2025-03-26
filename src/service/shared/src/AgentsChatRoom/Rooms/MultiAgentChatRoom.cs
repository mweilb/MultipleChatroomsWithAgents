 
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MultiAgents.AgentsChatRoom.WebSockets;
using MultiAgents.AzureAISpeech;
using MultiAgents.SemanticKernel.Modifications;
using MultiAgents.WebSockets;
using System.Net.WebSockets;
 

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace MultiAgents.AgentsChatRoom.Rooms
{
    /// <summary>
    /// An abstract multi-agent chat room handler that leverages an agent registry to load agents
    /// and an agent chat room to manage conversation state and streaming responses.
    /// This class encapsulates the logic to initialize agents, configure them with the Semantic Kernel,
    /// and handle WebSocket communications including streaming responses and error handling.
    /// </summary>
    public abstract class MultiAgentChatRoom : IMultiAgentChatRoom
    {
        // Private fields to hold dependencies for the agent registry, chat room, and logging.
  
        private AgentStreamingChatRoom? chatRoom;
        private ILogger<IMultiAgentChatRoom>? logger;

        /// <summary>
        /// Gets the command name associated with this handler (e.g., "groupchat").
        /// Derived classes must specify a unique command name.
        /// </summary>
        public abstract string Name { get; set; }

        public abstract string GroupName { get; set; }

        /// <summary>
        /// Gets the emoji representation for this handler.
        /// Derived classes can use an emoji to visually identify the chat room type.
        /// </summary>
        public abstract string Emoji { get; set; }

        /// <summary>
        /// Initializes the handler with its required dependencies:
        /// the agent registry to load agents, the chat room to manage conversation state,
        /// and a logger for tracking events and errors.
        /// </summary>
        /// <param name="agentRegistry">The registry containing agent definitions.</param>
        /// <param name="chatRoom">The chat room instance that manages conversation state and streaming responses.</param>
        /// <param name="logger">Logger instance for diagnostic purposes.</param>
        public void Initialize(
            AgentStreamingChatRoom chatRoom,
            ILogger<IMultiAgentChatRoom> logger)
        {
            
            this.chatRoom = chatRoom;
            this.logger = logger;
        }

        

        /// <summary>
        /// Main entry point for handling an incoming WebSocket command.
        /// Processes the message by adding it to the chat history and streaming agent responses back via a WebSocket sender.
        /// </summary>
        /// <param name="message">The incoming WebSocket message containing user content and metadata.</param>
        /// <param name="webSocket">The WebSocket connection used for sending responses back to the client.</param>
        /// <returns>A task representing the asynchronous handling operation.</returns>
        public async Task<(bool, string, string,string)> HandleCommandAsync(string author, WebSocketBaseMessage message, WebSocket webSocket, IAgentSpeech speech)
        {
            // Wrap the WebSocket connection with a sender helper to simplify sending messages.
            var sender = new WebSocketSender(webSocket);

            // Create a cancellation token to manage operation lifetime.
            using var cts = new CancellationTokenSource();
            CancellationToken cancellationToken = cts.Token;

            // Ensure that the chat room is properly initialized.
            if (chatRoom == null)
            {
                logger?.LogError("ChatRoom not initialized for {CommandName}", Name);
                await SendErrorAsync(sender, message.UserId, GroupName, $"ChatRoom not initialized {Name}", cancellationToken);
                return (false,"","", message.TransactionId);
            }

            try
            {
                var messageContent = new ChatMessageContent(AuthorRole.User, message.Content);
                messageContent.AuthorName = author;

                //check message by moderator
                EngageModerator(sender, message.UserId, message.Action, message.TransactionId, message.Content);

                // Add the user's incoming message to the conversation history.
                chatRoom.AddChatMessage(messageContent);

                // Begin streaming agent replies back to the client.
                return await StreamAgentRepliesAsync(message, sender, speech, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log any exceptions and send an error response to the client.
                logger?.LogError(ex, "Error occurred handling command {CommandName}", Name);
                await SendErrorAsync(sender, message.UserId, GroupName, $"Initialization or logic error: {ex.Message}", cancellationToken);
            }
            return (false, "", "", message.TransactionId);
        }

        /// <summary>
        /// Resets the chat room state by clearing the conversation history.
        /// </summary>
        /// <returns>A task that represents the asynchronous reset operation.</returns>
        public async Task Reset()
        {
            if (chatRoom != null)
            {
                await chatRoom.ResetAsync();
            }
            logger?.LogInformation("Chat history cleared for command: {CommandName}", Name);
        }


        // Private helper method that checks for complete sentences (ending with punctuation)
        // in the new text and sends them as speech. Returns the updated last index.
        private async Task<int> SendPendingSpeechAsync(
            string userId,
            string sendName,
            string fullText,
            int lastIndex,
            IWebSocketSender sender,
            IAgentSpeech speech,
            CancellationToken cancellationToken)
        {
            // Get the portion of text that hasn't been spoken yet.
            string newText = fullText.Substring(lastIndex);

            // Variables to count sentences and track the end of the desired text chunk.
            int sentenceCount = 0;
            int endIndex = -1;

            // Iterate through newText to find at least 4 sentence-ending punctuation marks.
            for (int i = 0; i < newText.Length; i++)
            {
                char currentChar = newText[i];
                if (currentChar == '.' || currentChar == '!' || currentChar == '?')
                {
                    sentenceCount++;
                    if (sentenceCount >= 4)
                    {
                        endIndex = i;
                        break;
                    }
                }
            }

            // If we found at least 4 sentences, extract and send the chunk.
            if (endIndex != -1)
            {
                // Extract the complete sentence(s) ending with punctuation.
                string sentenceChunk = newText.Substring(0, endIndex + 1);
                // Update the index to mark this text as spoken.
                lastIndex += sentenceChunk.Length;
                // Create and send a new speech message.
                var speechMessage = CreateNewMessage(userId, Guid.NewGuid().ToString(), sendName);
                speechMessage.Content = sentenceChunk;
                await sender.SendSpeachAsync(speechMessage, speech, cancellationToken);
            }

            return lastIndex;
        }


        // Private helper method to flush any remaining unsent text.
        private async Task FlushPendingSpeechAsync(
            string userId,
            string sendName,
            string fullText,
            int lastIndex,
            IWebSocketSender sender,
            IAgentSpeech speech,
            CancellationToken cancellationToken)
        {
            if (fullText.Length > lastIndex)
            {
                string pending = fullText.Substring(lastIndex);
                if (!string.IsNullOrWhiteSpace(pending))
                {
                    var speechMsg = CreateNewMessage(userId, Guid.NewGuid().ToString(), sendName);
                    speechMsg.Content = pending;
                    await sender.SendSpeachAsync(speechMsg, speech, cancellationToken);
                }
            }
        }

        // <summary>
        /// Streams agent responses to the client, handling new agent response initiation and updating the message with chunked output.
        /// Speech is sent incrementally as complete sentences become available.
        /// </summary>
        /// <param name="message">The original WebSocket message from the client.</param>
        /// <param name="sender">The WebSocket sender used to transmit responses back to the client.</param>
        /// <param name="speech">The speech synthesis interface used for TTS.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests during the streaming process.</param>
        /// <returns>A task representing the asynchronous streaming operation.</returns>
         // <summary>
        /// Streams agent responses to the client, handling new agent response initiation and updating the message with chunked output.
        /// Speech is sent incrementally as complete sentences become available.
        /// </summary>
        /// <param name="message">The original WebSocket message from the client.</param>
        /// <param name="sender">The WebSocket sender used to transmit responses back to the client.</param>
        /// <param name="speech">The speech synthesis interface used for TTS.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests during the streaming process.</param>
        /// <returns>A task representing the asynchronous streaming operation.</returns>
        protected virtual async Task<(bool, string, string, string)> StreamAgentRepliesAsync(
            WebSocketBaseMessage message,
            IWebSocketSender sender,
            IAgentSpeech speech,
            CancellationToken cancellationToken)
        {
            if (chatRoom == null)
            {
                logger?.LogError("ChatRoom not initialized for {CommandName}", Name);
                await SendErrorAsync(sender, message.UserId, GroupName, $"ChatRoom not initialized {Name}", cancellationToken);
                return (false, "", "", message.TransactionId);
            }

            try
            {
                // Create the initial message that will be updated with streaming content.
                WebSocketReplyChatRoomMessage currentMessage = CreateNewMessage(message.UserId, message.TransactionId, GroupName);
                currentMessage.RoomName = GroupName;
                currentMessage.SubRoomName = Name;
                currentMessage.UserTurn = false;
                // lastSpeechIndex tracks how many characters of currentMessage.Content have already been sent for speech.
                int lastSpeechIndex = 0;

                if (chatRoom.EnteringFirstTime == false)
                {
                    chatRoom.EnteringFirstTime = (message.SubAction == "start");
                }

                await foreach (var chunk in chatRoom.InvokeStreamingAsync(cancellationToken))
                {
                    if (chunk is AgentStreamingContent agentChunk)
                    {
                        if (agentChunk.RequestChatRoomChange)
                        {
                            //need to change chat rroms
                            int compareResult = string.Compare(agentChunk.RequestedChatRoom, "user", StringComparison.OrdinalIgnoreCase);
                            if (compareResult == 0)
                            {
                          
                                currentMessage.AgentName = "";
                                currentMessage.UserTurn = true;
                            
                                await sender.SendAsync(currentMessage, cancellationToken);
                                return (false, "", "", message.TransactionId);
                            }
                            
                            return (true, agentChunk.RequestedChatRoom, agentChunk.RequestedChatRoomContext, currentMessage.TransactionId);
                        }

                        // Update the message content from agent hints if available.
                        if (agentChunk.Hints.TryGetValue("agent", out var agentObj) &&
                            agentObj is Dictionary<string, string> agentHints &&
                            agentHints.TryGetValue("content", out string? content))
                        {
                            currentMessage.Content = content;
                        }

                        // When a new agent response starts, flush any pending text.
                        if (agentChunk.IsNewAgent)
                        {
                            if (currentMessage.Content.Length > lastSpeechIndex)
                            {
                                await FlushPendingSpeechAsync(message.UserId, GroupName, currentMessage.Content, lastSpeechIndex, sender, speech, cancellationToken);
                                
                                lastSpeechIndex = currentMessage.Content.Length;                                
                            }

                            //check content for moderation
                            EngageModerator(sender, currentMessage.AgentName, currentMessage.Action, currentMessage.TransactionId, currentMessage.Content);

                            // Start a new message for the new agent.
                            currentMessage = CreateNewMessage(message.UserId, Guid.NewGuid().ToString(), GroupName);

                            currentMessage.SubRoomName = Name;
                            currentMessage.RoomName = GroupName;
                            lastSpeechIndex = 0;
                        }

                        // Update the message with the new content chunk.
                        UpdateMessage(currentMessage, agentChunk);

                        // Check and send any complete sentence(s) if available.
                        lastSpeechIndex = await SendPendingSpeechAsync(message.UserId, GroupName, currentMessage.Content, lastSpeechIndex, sender, speech, cancellationToken);

                        // Send the updated message back to the client.
                        await sender.SendAsync(currentMessage, cancellationToken);
                    }
                }


              
                //check content for moderation
                EngageModerator(sender, currentMessage.AgentName, currentMessage.Action, currentMessage.TransactionId, currentMessage.Content);


                // At end-of-stream, flush any remaining unsent speech.
                await FlushPendingSpeechAsync(message.UserId, GroupName, currentMessage.Content, lastSpeechIndex, sender, speech, cancellationToken);
            }
            catch (Exception ex)
            {
                await SendErrorAsync(sender, message.UserId, GroupName, $"Error: {ex.Message}", cancellationToken);
                logger?.LogError(ex, "Error during streaming for command: {CommandName}", Name);
            }

            return (false, "", "", message.TransactionId);
        }

        /// <summary>
        /// Sends an error message to the client over the WebSocket.
        /// This helper method creates a standardized error message and sends it using the provided sender.
        /// </summary>
        /// <param name="sender">The WebSocket sender used to transmit messages.</param>
        /// <param name="userId">The user ID to which the error message is sent.</param>
        /// <param name="command">The command context for the error.</param>
        /// <param name="error">A detailed error message.</param>
        /// <param name="cancellationToken">Token to observe cancellation requests.</param>
        /// <returns>A task representing the asynchronous send operation.</returns>
        protected virtual async Task SendErrorAsync(
            IWebSocketSender sender,
            string userId,
            string command,
            string error,
            CancellationToken cancellationToken)
        {
            var errorResponse = CreateError(userId, GroupName, error);
            await sender.SendAsync(errorResponse, cancellationToken);
        }

        /// <summary>
        /// Creates a new WebSocket reply message for use in streaming agent responses.
        /// This method initializes the message with default values.
        /// </summary>
        /// <param name="userId">The ID of the user sending the message.</param>
        /// <param name="transactionId">A unique identifier for the transaction.</param>
        /// <param name="command">The command context associated with the message.</param>
        /// <returns>A new instance of <see cref="WebSocketReplyChatRoomMessage"/>.</returns>
        public static WebSocketReplyChatRoomMessage CreateNewMessage(
            string userId,
            string transactionId,
            string command
        ) => new()
        {
            UserId = userId,
            TransactionId = transactionId,
            Action = command,
            SubAction = "chunk",
            Content = string.Empty,
            AgentName = "Unknown",
        };

        /// <summary>
        /// Creates an error message formatted for WebSocket transmission.
        /// </summary>
        /// <param name="userId">The target user ID for the error.</param>
        /// <param name="command">The command associated with the error.</param>
        /// <param name="explanation">A detailed explanation of the error.</param>
        /// <returns>A new instance of <see cref="WebSocketReplyChatRoomMessage"/> representing the error.</returns>
        public static WebSocketReplyChatRoomMessage CreateError(
            string userId,
            string command,
            string explanation
        ) => new()
        {
            UserId = userId,
            TransactionId = Guid.NewGuid().ToString(),
            Action = command,
            SubAction = "error",
            Content = explanation,
            AgentName = string.Empty,
        };

        /// <summary>
        /// Updates a WebSocket reply message with new streaming content.
        /// It sets the agent name, updates hints, and maintains the "chunk" subaction.
        /// </summary>
        /// <param name="currentMessage">The current message to be updated.</param>
        /// <param name="content">The latest streaming content from an agent.</param>
        internal void UpdateMessage(WebSocketReplyChatRoomMessage currentMessage, AgentStreamingContent content)
        {
            // Set the agent name if available; otherwise, default to "Deciding..."
            currentMessage.AgentName = content.AgentName ?? "Deciding...";
            
            //find the agents in list and if not found, default to null
             currentMessage.Emoji = GetEmoji(content.AgentName ?? "");

            // Update the hints provided by the agent.
            currentMessage.Hints = new(content.Hints);
            // Ensure the subaction remains "chunk" for streaming.
            currentMessage.SubAction = "chunk";
        }

        internal virtual string GetEmoji(string Name)
        {
            return "🤔";
        }

        //empty moderator, dervived class to implement
        public virtual void EngageModerator(IWebSocketSender sender, string userId, string command, string transactionId, string textToModerate)
        {
            
        }
    }
}


#pragma warning restore SKEXP0110
#pragma warning restore SKEXP0001