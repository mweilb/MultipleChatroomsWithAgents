 
 
// using DocumentFormat.OpenXml.Vml.Office; // Removed if unused
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;

using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace MultiAgents.SemanticKernel.Modifications
{
    /// <summary>
    /// Represents a chat room that streams agent responses in a multi-agent conversation.
    /// It utilizes custom termination and selection strategies and leverages a Semantic Kernel
    /// for prompt invocations.
    /// </summary>
    public sealed class AgentStreamingChatRoom(IAgentStrategies roomConfig, Kernel kernel) : AgentChat
    {
        private readonly IAgentStrategies _strategies = roomConfig;
        private readonly Kernel _kernel = kernel;

        public string Name { get; set; } = string.Empty;

        // Holds the collection of chat agents. This should be set via InitGroupChat.
        public IReadOnlyCollection<Agent>? _agents;

        // Indicates whether the conversation is complete.
        public bool IsComplete = false;

        public string LastTermination = string.Empty;

        // Gets the list of agents as a list of Agent objects.
        public override IReadOnlyList<Agent> Agents => _agents?.Cast<Agent>().ToList() ?? new List<Agent>();

        public bool EnteringFirstTime { get; internal set; } = true;

        /// <summary>
        /// Initializes the group chat by setting the available agents.
        /// </summary>
        /// <param name="agents">A collection of configured chat completion agents.</param>
        /// <exception cref="InvalidOperationException">Thrown if no agents are provided.</exception>
        public void InitGroupChat(IReadOnlyCollection<ChatCompletionAgent> agents)
        {
            if (!agents.Any())
            {
                throw new InvalidOperationException("No agents have been loaded. Please configure them first.");
            }
            _agents = agents;
        }

        /// <summary>
        /// Resets the conversation history.
        /// </summary>
        public void Reset() => History.Clear();

        /// <summary>
        /// Adds a user message to the conversation history.
        /// </summary>
        /// <param name="userMessage">The message content from the user.</param>
        public async Task AddUserMessageAsync(string userMessage)
        {
            var entry = new ChatMessageContent(AuthorRole.User, userMessage)
            {
                AuthorName = "User"
            };

            History.Add(entry);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Streams the conversation by invoking agent responses iteratively.
        /// It uses selection and termination strategies to determine the flow of conversation.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An asynchronous stream of StreamingChatMessageContent items.</returns>
        public override async IAsyncEnumerable<StreamingChatMessageContent> InvokeStreamingAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_agents == null || !_agents.Any())
            {
                throw new InvalidOperationException("No agents have been loaded. Please initialize the agents first.");
            }
            var agentsList = _agents.Cast<Agent>().ToList();
            bool conversationComplete = false;
            Agent? lastAgent = null;
            const int maxIterations = 100;

            for (int iteration = 0; !conversationComplete && iteration < maxIterations; iteration++)
            {
                // Retrieve streaming strategies based on the current agent.
                var (terminationStrategy, selectionStrategy) = _strategies.GetStreamingStrategies(lastAgent, LastTermination, this.EnteringFirstTime);
                this.EnteringFirstTime = false;

                Agent? selectedAgent = null;
                var streamingContent = new AgentStreamingContent { IsNewAgent = true };
                yield return streamingContent;
                streamingContent.IsNewAgent = false;

                // Execute the selection strategy to pick an agent.
                await foreach (var agentCandidate in selectionStrategy.SelectAgentStreaming(streamingContent, agentsList, History, cancellationToken))
                {
                    selectedAgent = agentCandidate;
                    yield return streamingContent;
                }

                // If no agent is selected, yield content and break.
                if (selectedAgent == null)
                {
                    yield return streamingContent;
                    break;
                }

                // Update the last agent for the next iteration.
                lastAgent = selectedAgent;
                streamingContent.AgentName = selectedAgent.Name;
                yield return new AgentStreamingContent(streamingContent);

                if (selectedAgent is ChatHistoryKernelAgent chatAgent)
                {
                    // Stream the agent's response.
                    await foreach (var (prompt, responseContent, reasoning) in StreamAgentResponseAsync(chatAgent, cancellationToken))
                    {
                        var hintData = new Dictionary<string, string>
                        {
                            ["prompt"] = prompt,
                            ["content"] = responseContent,
                            ["reasons"] = reasoning
                        };

                        streamingContent.Hints["agent"] = hintData;
                        yield return new AgentStreamingContent(streamingContent);
                    }
                }

                // Check if termination strategy signals that conversation should end.
                await foreach (var shouldTerminate in terminationStrategy.ShouldAgentTerminateStreaming(streamingContent, selectedAgent, History, cancellationToken))
                {
                    conversationComplete = shouldTerminate;
                    yield return streamingContent;
                }

                if (conversationComplete)
                {
                    LastTermination = terminationStrategy.Name;
                    break;
                }
            }
        }

        /// <summary>
        /// Streams the response from the selected agent.
        /// It builds a prompt from the conversation history and the agent's instructions,
        /// then returns the streaming response content.
        /// </summary>
        /// <param name="agent">The agent whose response is being streamed.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>An asynchronous stream of tuples containing the prompt, plain text response, and thinking text.</returns>
        private async IAsyncEnumerable<(string Prompt, string Response, string Thinking)> StreamAgentResponseAsync(ChatHistoryKernelAgent agent, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string promptContent = string.Join("\n", History.Select(msg => $"{msg.AuthorName}: {msg.Content}"));
            string prompt = agent.Instructions ?? "missing prompt";

            if (string.IsNullOrEmpty(prompt))
            {
                yield return ("prompt is empty!!", "error", "error as prompt is empty");
            }
            else
            {
                var overallResponse = new StringBuilder();
                string updatedPlainText = string.Empty;
                string updatedThinkingText = string.Empty;

                var arguments = new KernelArguments { { "query", promptContent }, { "messages", promptContent } };
                var promptTemplateFactory = new HandlebarsPromptTemplateFactory();

                var streamingResponse = _kernel.InvokePromptStreamingAsync(prompt, arguments,
                    templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                    promptTemplateFactory: promptTemplateFactory,
                    cancellationToken: cancellationToken);

                await foreach (var chunk in streamingResponse)
                {
                    string currentChunk = chunk.ToString() ?? string.Empty;
                    overallResponse.Append(currentChunk);
                    (updatedPlainText, updatedThinkingText) = OllamaHelper.SplitContentFromThinking(overallResponse.ToString());
                    yield return (prompt, updatedPlainText, updatedThinkingText);
                }

                var responseEntry = new ChatMessageContent(AuthorRole.User, updatedPlainText)
                {
                    AuthorName = agent.Name
                };

                History.Add(responseEntry);
            }
        }

        /// <summary>
        /// Not implemented for streaming; use InvokeStreamingAsync instead.
        /// </summary>
        public override IAsyncEnumerable<ChatMessageContent> InvokeAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();

    }
}

#pragma warning restore SKEXP0001
#pragma warning restore SKEXP0110
