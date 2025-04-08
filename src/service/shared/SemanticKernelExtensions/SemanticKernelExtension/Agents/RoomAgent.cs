using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelExtension.Agents
{
    public class RoomAgent : EchoAgent
    {
        private readonly string _instructionToSummary;
        private readonly Kernel _kernel;

        public RoomAgent(
            string name,
            string agentName,
            string modelId,
            string message,
            bool visible,
            Kernel kernel,
            string summarizeInstructions = "Summarize the Conversations as concise as possible and dont try to answer questions."
        ) : base(name, agentName, modelId, message, visible)
        {
            _instructionToSummary = summarizeInstructions;
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel), "Kernel is required to invoke the summary.");
        }

        public async IAsyncEnumerable<StreamingChatMessageContent> InvokeSummaryStreamingAsync(
            ChatMessageContent[] history,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Validate the kernel instance
            if (_kernel is null)
            {
                throw new InvalidOperationException("Kernel is required to invoke the summary.");
            }

            // Initialize a new chat history and add existing messages
            var newHistory = new ChatHistory();
            foreach (var message in history)
            {
                newHistory.Add(message);
            }

            // Prepend the summarization instruction as a system message
            newHistory.AddSystemMessage(_instructionToSummary);

            // Retrieve the chat completion service from the kernel
            var chatService = _kernel.GetRequiredService<IChatCompletionService>();

            // Define execution settings if needed
            var executionSettings = new PromptExecutionSettings
            {
                // Configure settings as required, e.g., Temperature = 0.7, MaxTokens = 150
            };

            // Stream the summary response from the model
            await foreach (var message in chatService.GetStreamingChatMessageContentsAsync(
                                                    newHistory,
                                                    executionSettings,
                                                    _kernel,
                                                    cancellationToken))
            {
                yield return message;
            }
        }

        internal string GetAgentName()
        {
           return _agentName;
        }
    }
}
