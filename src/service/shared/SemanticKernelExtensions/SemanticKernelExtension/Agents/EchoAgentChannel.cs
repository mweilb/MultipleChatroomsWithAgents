// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Diagnostics.CodeAnalysis;


namespace SemanticKernelExtension.Agents
{ 

    /// <summary>
    /// A <see cref="AgentChannel"/> specialization for use with <see cref="OpenAIAssistantAgent"/>.
    /// </summary>
    [Experimental("SKEXP0110")]
    internal sealed class EchoAgentChannel(ChatHistory history) : AgentChannel<EchoAgent>
    {

        ChatHistory _history = history;
        protected override IAsyncEnumerable<ChatMessageContent> GetHistoryAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

 
        protected override IAsyncEnumerable<(bool IsVisible, ChatMessageContent Message)> InvokeAsync(EchoAgent agent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync(EchoAgent agent, IList<ChatMessageContent> messages, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task ReceiveAsync(IEnumerable<ChatMessageContent> history, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override Task ResetAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        protected override string Serialize()
        {
            throw new NotImplementedException();
        }
    }
}