 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelExtension.Hacks;
using System.Runtime.CompilerServices;
using System.Text.Json;
 

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace SemanticKernelExtension.Agents
{
    public class EchoAgent : ChatHistoryAgent
    {
        private readonly string _message;
        private readonly bool _visible;
        protected readonly string _agentName;
        private readonly string _modelId;

        public EchoAgent(string name, string agentName, string modelId, string message, bool visible)
            : base()
        {
            _message = message;
            _visible = visible;
            _agentName = agentName;
            _modelId = modelId;
            Name = name;
        }

        public (string,string) TempFunctionUntilPortingIsOver() { return (_agentName, _message); }

        // 1) Non-streaming ChatMessageContent (history-based InvokeAsync)
        [Obsolete]
        public override async IAsyncEnumerable<ChatMessageContent> InvokeAsync(
            ChatHistory history,
            KernelArguments? arguments = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var response = new ChatMessageContent
            {
                AuthorName = _agentName,
                Content = _message,
                Role = _visible ? AuthorRole.System : AuthorRole.Tool,
                ModelId = _modelId
            };

            yield return response;
        }

        // 2) Non-streaming ChatMessageContent (messages-based InvokeAsync)
        public override async IAsyncEnumerable<AgentResponseItem<ChatMessageContent>> InvokeAsync(
            ICollection<ChatMessageContent> messages,
            AgentThread? thread = null,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var response = new ChatMessageContent
            {
                AuthorName = _agentName,
                Content = _message,
                Role = _visible ? AuthorRole.System : AuthorRole.Tool,
                ModelId = _modelId
            };

            yield return new AgentResponseItem<ChatMessageContent>(response, thread!);

            if (_visible)
            {
                messages.Add(response);
            }

        }

        // 3) Streaming ChatMessageContent (history-based InvokeStreamingAsync)
        [Obsolete]
        public override async IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync(
            ChatHistory history,
            KernelArguments? arguments = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var response = new StreamingChatMessageContent(
                _visible ? AuthorRole.System : AuthorRole.Tool,
                _message, modelId: _modelId)
            {
                AuthorName = _agentName
            };


            yield return response;

            if (_visible)
            {
                var content = new ChatMessageContent
                {
                    AuthorName = _agentName,
                    Content = _message,
                    Role = _visible ? AuthorRole.System : AuthorRole.Tool,
                    ModelId = _modelId
                };

                history.Add(content);
            }

        }

        // 4) Streaming ChatMessageContent (messages-based InvokeStreamingAsync)
        public override async IAsyncEnumerable<AgentResponseItem<StreamingChatMessageContent>> InvokeStreamingAsync(
            ICollection<ChatMessageContent> messages,
            AgentThread? thread = null,
            AgentInvokeOptions? options = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;

            var response = new StreamingChatMessageContent(
                _visible ? AuthorRole.System : AuthorRole.Tool,
                _message,
                modelId:_modelId)
            {
                AuthorName = _agentName,
            };

            yield return new AgentResponseItem<StreamingChatMessageContent>(response, thread!);
            if (_visible)
            {
                var content = new ChatMessageContent
                {
                    AuthorName = _agentName,
                    Content = _message,
                    Role = _visible ? AuthorRole.System : AuthorRole.Tool,
                    ModelId = _modelId
                };

                messages.Add(content);
            }
        }

        // 5) Restoring channel logic
        protected override Task<AgentChannel> RestoreChannelAsync(
            string channelState,
            CancellationToken cancellationToken)
        {
            ChatHistory history = JsonSerializer.Deserialize<ChatHistory>(channelState)
                ?? throw new KernelException("Unable to restore channel: invalid state.");
            return Task.FromResult<AgentChannel>(new EchoAgentChannel(history));
        }
    }
}
