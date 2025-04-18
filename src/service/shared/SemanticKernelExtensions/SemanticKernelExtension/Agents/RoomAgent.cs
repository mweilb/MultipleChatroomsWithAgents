﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelExtension.Hacks;
using SemanticKernelExtension.Orchestrator;
 
using System.Runtime.CompilerServices;
using System.Text;
 

namespace SemanticKernelExtension.Agents
{
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
    public class RoomAgent : EchoAgent
    {
        private string _instructionToSummary;
 
        private readonly bool _yieldOnRoomChange = false;
        private readonly string _yieldCanceledName = string.Empty;

        /// <summary>
        /// Suggests whether the room should yield or not.
        /// Override this method to provide custom yield logic.
        /// </summary>
        /// <returns>True if the room should yield, otherwise false.</returns>
        public virtual bool ShouldYield(string roomName)
        {
            return _yieldOnRoomChange;
        }

        public virtual void ReportRoomChangeRquest(bool changed)
        {
           
        }

        public void setInstructions(string instructions)
        {
            _instructionToSummary = instructions;
        }


        private readonly Kernel _kernel;

        public RoomAgent(
            string name,
            string agentName,
            string modelId,
            string message,
            bool visible,
            Kernel kernel,
            bool yieldOnRoomChange = false,
            string yieldCanceledName = "System",
            string summarizeInstructions = "Summarize the Conversations as concise as possible and dont try to answer questions."
        ) : base(name, agentName, modelId, message, visible)
        {
            _instructionToSummary = summarizeInstructions;
            _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel), "Kernel is required to invoke the summary.");
            _yieldOnRoomChange = yieldOnRoomChange;
            _yieldCanceledName = yieldCanceledName;
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

        /// <summary>
        /// Summarizes the chat history of the previous room and adds it as a system message in the new active chat room.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of <see cref="StreamingOrchestratorContent"/> representing the summary process.</returns>
        public virtual async IAsyncEnumerable<StreamingOrchestratorContent> RespondToRoomChange(string orchestratorName, string currentChatRoomName, AgentGroupChat currentChatRoom, AgentGroupChat lastChatRoom,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {


            if (currentChatRoom != lastChatRoom)
            {
                await foreach (var agentChunk in SuccesfullChangedRoom(orchestratorName, currentChatRoomName, currentChatRoom, lastChatRoom, cancellationToken))
                {
                    // Yield each chunk as it's received
                    yield return agentChunk;
                }
            }
            else
            {
                if (currentChatRoom != null)
                {
                    currentChatRoom.AddChatMessage(new ChatMessageContent(AuthorRole.System, "User Canceled Room Change")
                    {
                        AuthorName = this._yieldCanceledName,
                    });
                }
            }
        }

        protected async IAsyncEnumerable<StreamingOrchestratorContent> SuccesfullChangedRoom(string orchestratorName, string currentChatRoomName, AgentGroupChat currentChatRoom, AgentGroupChat lastChatRoom, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Retrieve and reverse the chat history from the previous room
            var lastHistory = await lastChatRoom.GetChatMessagesAsync(cancellationToken)
                                                 .Reverse()
                                                 .ToArrayAsync(cancellationToken);

            bool isFirstMessage = true;
            var summaryBuilder = new StringBuilder();

            // Generate the summary asynchronously
            await foreach (var agentChunk in this.InvokeSummaryStreamingAsync(lastHistory, cancellationToken))
            {
                // Accumulate the summary content
                summaryBuilder.Append(agentChunk.Content);

                // Yield each chunk as it's received
                yield return new StreamingOrchestratorContent(
                    isFirstMessage ? StreamingOrchestratorContent.ActionTypes.RoomMessageStarted : StreamingOrchestratorContent.ActionTypes.RoomMessageUpdated,
                    orchestratorName,
                    currentChatRoomName,
                    this.Name ?? "Previous Room",
                    false,
                    agentChunk
                );

                isFirstMessage = false;
            }

            // Finalize the summary message
            var consolidatedSummary = summaryBuilder.ToString().Trim();

            if (!string.IsNullOrEmpty(consolidatedSummary))
            {
                // Add the consolidated summary to the new active chat room as a system message
                var newChatRoom = currentChatRoom;
                if (newChatRoom != null)
                {
                    newChatRoom.AddChatMessage(new ChatMessageContent(AuthorRole.Assistant, consolidatedSummary)
                    {
                        AuthorName = this.GetAgentName()
                    });

                }
            }

            // Yield the finalization action
            yield return new StreamingOrchestratorContent(
                StreamingOrchestratorContent.ActionTypes.RoomMessageFinished,
                orchestratorName,
                currentChatRoomName,
                this.Name ?? "Previous Room",
                false,
                null
            );
        }
    }
}
