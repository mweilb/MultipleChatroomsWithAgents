using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelExtension.Agents; 
using System.Runtime.CompilerServices;
 

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticKernelExtension.Orchestrator
{
    /// <summary>
    /// Orchestrates group chat interactions for agents.  
    /// Manages a collection of <see cref="AgentGroupChat"/> instances, room switches, and chat messages.
    /// </summary>
    public class AgentGroupChatOrchestrator
    {
        // Dictionary mapping chat names to AgentGroupChat instances.
        private readonly Dictionary<string, AgentGroupChat> _chats = new();

        // Caches the logger instance.
        private ILogger? _logger;

        // Tracks the name of the currently active chat.
        private string _activeChatName = string.Empty;

        // Stores previous room data for generating summaries if needed.
        private AgentGroupChat? _lastChatRoom = null;
        private RoomAgent? _lastRoomAgent = null;
        private string _lastRoomName = string.Empty;

        /// <summary>
        /// Gets the <see cref="ILoggerFactory"/> for creating loggers.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; init; } = NullLoggerFactory.Instance;

        // The name of the orchestrator.
        public string Name = string.Empty;

        /// <summary>
        /// Gets the logger instance for this orchestrator.
        /// </summary>
        protected ILogger Logger => _logger ??= LoggerFactory.CreateLogger(GetType());

        // Orchestrator group name.
        public string OrchestratorName = string.Empty;

        // Indicates if the orchestrator should yield (instead of auto-switch) on room change.
        public bool YieldOnRoomChange = false;

        // Identifier for the room model.
        public string RoomModelId { get; set; } = "room";

        // Default author name for chat messages when one is not provided.
        public string UndefinedAuthorName { get; set; } = "undefined";

        private string? _startRoom;

        public AgentGroupChatOrchestrator() { }

        /// <summary>
        /// Gets the currently active chat.
        /// </summary>
        public AgentGroupChat? ActiveChat =>
            _chats.TryGetValue(_activeChatName, out var chat) ? chat : null;

        /// <summary>
        /// Exposes all current chats.
        /// </summary>
        public IReadOnlyDictionary<string, AgentGroupChat> AllChats => _chats;

        /// <summary>
        /// Adds a chat with the given name.  
        /// If the orchestrator does not yet have an active chat, this chat is set active.
        /// </summary>
        /// <param name="name">Unique chat name.</param>
        /// <param name="chat">The AgentGroupChat instance.</param>
        /// <returns>True if added; false if a chat with that name already exists.</returns>
        public bool Add(string name, AgentGroupChat chat)
        {
            if (_chats.ContainsKey(name))
            {
                return false;
            }

            _chats[name] = chat;
            // If no active chat has been assigned yet, set it.
            if (string.IsNullOrEmpty(_activeChatName))
            {
                _activeChatName = name;
            }
            return true;
        }

        /// <summary>
        /// Sets the starting room for the orchestrator.
        /// </summary>
        /// <param name="name">Name of the room to set active.</param>
        /// <returns>True if the room exists and is set active; otherwise, false.</returns>
        public bool SetStartRoom(string name)
        {
            if (name != _activeChatName && _chats.ContainsKey(name))
            {
                _activeChatName = name;
                _startRoom = name;   
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles a user request to switch rooms.
        /// Logs differently depending on whether the request matches the current active chat.
        /// </summary>
        /// <param name="name">The requested room name.</param>
        /// <returns>True if the room switch was successful; otherwise, false.</returns>
        public bool UserRequestSwitchTo(string name)
        {
            if (_lastChatRoom != null && _lastRoomAgent != null)
            {
                if (name == _activeChatName)
                {
                    Logger.LogInformation("User canceled room switch '{0}'", name);
                }
                else
                {
                    Logger.LogInformation("User requested to switch to room '{0}'", name);
                }
            }
            return SwitchTo(name);
        }

        /// <summary>
        /// Switches to the specified room if available.
        /// Logs the room change selection or cancellation accordingly.
        /// </summary>
        /// <param name="name">Name of the room to switch to.</param>
        /// <returns>True if switched; otherwise, false.</returns>
        private bool SwitchTo(string name)
        {
            if (name != _activeChatName)
            {
                if (_chats.ContainsKey(name))
                {
                    _activeChatName = name;
                    _logger?.LogRoomChangeSelected(OrchestratorName, nameof(SwitchTo), name);
                    return true;
                }
            }

            _logger?.LogRoomChangeCanceled(OrchestratorName, nameof(SwitchTo), name);
            _lastChatRoom = null;
            _lastRoomAgent = null;
            return false;
        }

        /// <summary>
        /// Returns the name of the active chat.
        /// </summary>
        public string? GetActiveChatName() => _activeChatName;

        /// <summary>
        /// Removes the specified chat.  
        /// If the removed chat is the active chat, the active chat is reassigned to the first available chat.
        /// </summary>
        /// <param name="name">The name of the chat to remove.</param>
        /// <returns>True if removed; otherwise, false.</returns>
        public bool Remove(string name)
        {
            if (!_chats.ContainsKey(name))
            {
                return false;
            }

            bool removed = _chats.Remove(name);
            if (removed && _activeChatName.Equals(name, System.StringComparison.Ordinal))
            {
                _activeChatName = _chats.Any() ? _chats.Keys.First() : string.Empty;
            }
            return removed;
        }

        /// <summary>
        /// Sets the group name for this orchestrator.
        /// </summary>
        /// <param name="groupName">Group name.</param>
        public void SetGroupName(string groupName)
        {
            OrchestratorName = groupName;
        }

        /// <summary>
        /// Adds a user chat message to the active chat.
        /// </summary>
        /// <param name="message">The message text.</param>
        /// <returns>True if the message was added successfully; otherwise, false.</returns>
        public bool AddChatMessage(string message)
        {
            var agentGroupChat = ActiveChat;
            if (agentGroupChat == null)
            {
                _logger?.LogError("No active AgentGroupChat is selected. Cannot add user message.");
                return false;
            }

            agentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, message) { AuthorName = "User" });
            _logger?.LogInformation("Added user message to chat '{0}': {1}", _activeChatName, message);
            return true;
        }

        /// <summary>
        /// Invokes the streaming process for the active chat and yields updates (start/update/end/error).  
        /// Also handles room-change events by summarizing previous room chats and switching rooms as needed.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An async stream of <see cref="StreamingOrchestratorContent"/> messages.</returns>
        public async IAsyncEnumerable<StreamingOrchestratorContent> InvokeStreamingAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            bool roomChanged;
            string newRoomName;

            do
            {
                var agentGroupChat = ActiveChat;

                // Exit if no active chat is selected.
                if (agentGroupChat == null)
                {
                    _logger?.LogError("No active AgentGroupChat selected.");
                    var errorContent = new StreamingChatMessageContent(AuthorRole.System, "No active AgentGroupChat selected.");
                    yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.Error, OrchestratorName, _activeChatName, string.Empty,false, errorContent);
                    yield break;
                }

                // If there is a previous room pending, summarize its chat.
                if (_lastChatRoom != null && _lastRoomAgent != null && _lastChatRoom != agentGroupChat)
                {
                    _logger?.LogInformation("Started summary generation for RoomAgent: {0}", _lastRoomName);
                    await foreach (var content in _lastRoomAgent.SummarizeAndIntegratePreviousRoomChatAsync(OrchestratorName, _activeChatName, agentGroupChat, _lastChatRoom, cancellationToken))
                    {
                        yield return content;
                    }
                }

                // Reset previous room tracking.
                _lastChatRoom = null;
                _lastRoomAgent = null;

                roomChanged = false;
                newRoomName = string.Empty;
                string currentAgent = string.Empty;
                agentGroupChat.IsComplete = false;

                // Process streaming chunks from the active chat.
                await foreach (var agentChunk in agentGroupChat.InvokeStreamingAsync(cancellationToken))
                {
                    // Assign a default author name if none is provided.
                    if (string.IsNullOrEmpty(agentChunk.AuthorName))
                    {
                        agentChunk.AuthorName = UndefinedAuthorName;
                    }

                    // When the agent changes, finish the previous one and start a new section.
                    if (!currentAgent.Equals(agentChunk.AuthorName, System.StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(currentAgent))
                        {
                            yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.AgentFinsihed, OrchestratorName, _activeChatName, currentAgent);
                        }

                        // Check for a room change signal.
                        if (agentChunk.Role == AuthorRole.Tool && agentChunk.ModelId == RoomModelId)
                        {
                            newRoomName = agentChunk.Content ?? string.Empty;
                            Logger.LogRoomChangeStarted(OrchestratorName, nameof(InvokeStreamingAsync), _activeChatName, newRoomName);
                            _lastRoomName = _activeChatName;
                            Logger.LogInformation("Room Agent was selected: '{0}'", newRoomName);

        
                            // Find and assign the corresponding room agent.
                            var matchingAgent = agentGroupChat.Agents.FirstOrDefault(a =>
                                !string.IsNullOrEmpty(a.Name) &&
                                a.Name.Equals(newRoomName, System.StringComparison.OrdinalIgnoreCase));

                            if (matchingAgent is RoomAgent roomAgent)
                            {
                                _lastChatRoom = agentGroupChat;
                                _lastRoomAgent = roomAgent;

                                // Respect the RoomAgent's ShouldYield decision
                                YieldOnRoomChange = roomAgent.ShouldYield();
                               
                            }

                            yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.RoomChange, OrchestratorName, _activeChatName, currentAgent, YieldOnRoomChange, agentChunk);


                            roomChanged = true;
                            break;
                        }

                        currentAgent = agentChunk.AuthorName;
                        yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.AgentStarted, OrchestratorName, _activeChatName, currentAgent,false,  agentChunk);
                        continue;
                    }

                    yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.AgentUpdated, OrchestratorName, _activeChatName, currentAgent, false, agentChunk);
                }

                if (!string.IsNullOrEmpty(currentAgent))
                {
                    yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.ActionTypes.AgentFinsihed, OrchestratorName, _activeChatName, currentAgent);
                }

                // If a room change was detected, either auto switch or wait based on the YieldOnRoomChange flag.
                if (roomChanged)
                {
                    if (!YieldOnRoomChange)
                    {
                        Logger.LogInformation("Auto switching room to room '{0}'", newRoomName);
                        SwitchTo(newRoomName);
                    }
                    else
                    {
                        Logger.LogInformation("Waiting for input to switch to room '{0}'", newRoomName);
                    }
                }

            } while (roomChanged && !cancellationToken.IsCancellationRequested);
        }

        /// <summary>
        /// Resets all chats asynchronously.
        /// </summary>
        /// <returns>True if all chats reset successfully; otherwise, false.</returns>
        public async Task<bool> ResetAsync()
        {
            try
            {
                foreach (var chat in _chats.Values)
                {
                    await chat.ResetAsync();
                }



            }
            catch (System.Exception ex)
            {
                _logger?.LogError(ex, "Error resetting chats");
                return false;
            }

            // Reassign active chat based on the remembered start room if set,
            // or fallback to the first available chat.
            if (!string.IsNullOrEmpty(_startRoom) && _chats.ContainsKey(_startRoom))
            {
                _activeChatName = _startRoom;
                _logger?.LogInformation("Reset complete. Active room set to start room '{0}'", _startRoom);
            }
            else if (_chats.Any())
            {
                _activeChatName = _chats.Keys.First();
                _logger?.LogInformation("Reset complete. Active room reset to the first available room '{0}'", _activeChatName);
            }
            else
            {
                _activeChatName = string.Empty;
                _logger?.LogWarning("Reset complete, but no chats are available.");
            }


            return true;
        }
    }
}
