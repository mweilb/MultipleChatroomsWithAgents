using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;


#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticKernelExtension.Orchestrator
{

    public class AgentGroupChatOrchestrator
    {
        private readonly Dictionary<string, AgentGroupChat> _chats = [];
        private readonly ILogger<AgentGroupChatOrchestrator>? _logger;

        private string _activeChatName = string.Empty;
        
        public string OrchestratorName = string.Empty;
        public bool YieldOnRoomChange = false;

        public string RoomModelId { get; set; } = "Room";
        public string UndefinedAuthorName { get; set; } = "undefined";

        public AgentGroupChatOrchestrator(ILogger<AgentGroupChatOrchestrator>? logger = null)
        {
            _logger = logger;
        }

        public AgentGroupChat? ActiveChat =>
            _activeChatName != null && _chats.TryGetValue(_activeChatName, out var chat) ? chat : null;

        public IReadOnlyDictionary<string, AgentGroupChat> AllChats => _chats;

        public bool Add(string name, AgentGroupChat chat)
        {
            if (_chats.ContainsKey(name)) return false;
            _chats[name] = chat;
            _activeChatName ??= name;
            return true;
        }

        public bool SwitchTo(string name)
        {
            if (_chats.ContainsKey(name))
            {
                _activeChatName = name;
                return true;
            }
            return false;
        }

        public string? GetActiveChatName() => _activeChatName;

        public bool Remove(string name)
        {
            if (_chats == null || _chats.Count <= 0 || !_chats.ContainsKey(name)) return false;

            var removed = _chats.Remove(name);
            if (removed && _activeChatName == name)
            {
                _activeChatName = _chats.Count > 0
                    ? new List<string>(_chats.Keys)[0]
                    : string.Empty;
            }
            return removed;
        }

        public void SetGroupName(string groupName)
        {
            OrchestratorName = groupName;
        }

        public bool AddChatMessage(string message)
        {
            var agentGroupChat = ActiveChat;
            if (agentGroupChat == null)
            {
                _logger?.LogError("No active AgentGroupChat is selected. Cannot add user message.");
                return false;
            }

            agentGroupChat.AddChatMessage(new ChatMessageContent(AuthorRole.User, message) { AuthorName="User"});

            _logger?.LogInformation("Added user message to chat '{chatName}': {message}", _activeChatName, message);

            return true;
        }


        /// <summary>
        /// Invokes the streaming for the active chat and yields message updates (start/update/end/error).
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to stop streaming.</param>
        /// <returns>An async stream of <see cref="StreamingOrchestratorContent"/> objects.</returns>
        public async IAsyncEnumerable<StreamingOrchestratorContent> InvokeStreamingAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var agentGroupChat = ActiveChat;

            // If there's no active chat, yield an error and exit.
            if (agentGroupChat == null)
            {
                _logger?.LogError("No active AgentGroupChat selected.");
                var content = new StreamingChatMessageContent(AuthorRole.System, "No active AgentGroupChat selected.");
                yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.Error, OrchestratorName, _activeChatName, string.Empty, content);
                yield break;
            }

            bool roomChanged;
            string newRoomName;
            do
            {
                roomChanged = false;
                newRoomName = string.Empty;

                string currentAgent = string.Empty;
                agentGroupChat.IsComplete = false;

                await foreach (var agentChunk in agentGroupChat.InvokeStreamingAsync(cancellationToken))
                {
                    // Default the AuthorName if none is provided
                    if (string.IsNullOrEmpty(agentChunk.AuthorName))
                    {
                        agentChunk.AuthorName = UndefinedAuthorName;
                    }

                    // If we're switching to a new agent, yield an End for the previous agent 
                    // and a Start for the new agent
                    if (!currentAgent.Equals(agentChunk.AuthorName, StringComparison.OrdinalIgnoreCase))
                    {
                        // End event for the previous agent (only if it was non-empty)
                        if (!string.IsNullOrEmpty(currentAgent))
                        {
                            yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.AgentEnded, OrchestratorName, _activeChatName, currentAgent);
                        }

                        currentAgent = agentChunk.AuthorName;
                        yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.AgentEnded, OrchestratorName, _activeChatName, currentAgent, agentChunk);
                        continue;
                    }

                    // If it's a "tool" message with ModelId == "Room", we can stop streaming altogether
                    if (agentChunk.Role == AuthorRole.Tool && agentChunk.ModelId == RoomModelId)
                    {
                        newRoomName = agentChunk.Content ?? "";
                        yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.RoomChange, OrchestratorName, _activeChatName, currentAgent, agentChunk);
                        roomChanged = true;
                        break;
                    }


                    yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.AgentUpdated, OrchestratorName, _activeChatName, currentAgent, agentChunk);

                }

                if (!string.IsNullOrEmpty(currentAgent))
                {
                    yield return new StreamingOrchestratorContent(StreamingOrchestratorContent.Action.AgentEnded, OrchestratorName, _activeChatName, currentAgent);
                }

                if (roomChanged == true && YieldOnRoomChange == false)
                {
                    // Example: parse the new room name from the chunk text (if that's how you store it).
                    var success = SwitchTo(newRoomName);
                    if (success)
                    {
                        _logger?.LogInformation("Switched to new room: {NewRoom}", newRoomName);
                        roomChanged = true;
                    }
                    else
                    {
                        _logger?.LogError("Failed to switch to room: {NewRoom}", newRoomName);
                    }
                }



            } while (roomChanged && !cancellationToken.IsCancellationRequested);

        }

        public async Task<bool> ResetAsync()
        {
            try
            {
                foreach (var chat in _chats.Values)
                {
                    await chat.ResetAsync();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error resetting chats");
                return false;
            }

            return true;

        }
    }
}
