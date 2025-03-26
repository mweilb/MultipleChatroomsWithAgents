 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using MultiAgents.Configurations;
using MultiAgents.SemanticKernel.Modifications;
using System.Runtime.CompilerServices;

#pragma warning disable SKEXP0110

namespace MultiAgents.Agents.Strategies
{
    /// <summary>
    /// Implements a termination strategy using YAML configuration. This strategy filters the conversation history,
    /// then uses a termination description prompt to decide whether the current agent should terminate the conversation.
    /// </summary>
    public class YamlTerminationStrategy(Kernel kernel, YamlTerminationDecisionConfig? decision)
        : TerminationStreamingStrategy(decision?.Name)
    {

 

        private readonly YamlTerminationDecisionConfig? terminationInternal = decision;

        // Kernel instance used for invoking prompts.
        private readonly Kernel kernelInstance = kernel ?? throw new ArgumentNullException(nameof(kernel));

        // Optional list of precondition prompts to filter the conversation history.
        private readonly List<string>? preconditionPromptsInternal = decision?.MessagePresetFilters ?? [];

        // Optional filter instruction applied to the conversation history.
        private readonly string? filterInstructionInternal = decision?.MessagesFilter ?? "Empty Prompt";

        // The termination description prompt that outlines the conditions for termination.
        private readonly string terminationDescriptionPromptInternal = decision?.Prompt ?? "Empty Prompt";

        /// <summary>
        /// Streams the termination decision process by first filtering the conversation history and then invoking a decision prompt.
        /// Intermediate results are yielded until the final termination decision (true or false) is reached.
        /// </summary>
        /// <param name="streamingContent">Container for hints and intermediate results.</param>
        /// <param name="agent">The agent for which termination is being evaluated.</param>
        /// <param name="chatHistory">The conversation history.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous stream yielding boolean values that indicate whether to terminate.</returns>
        public override async IAsyncEnumerable<bool> ShouldAgentTerminateStreaming(
            AgentStreamingContent streamingContent,
            Agent agent,
            IReadOnlyList<ChatMessageContent> chatHistory,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            string filteredHistoryJson = string.Empty;

            //apply presets as they allow things to run faster sometimes
            if (agent.Name != null && terminationInternal != null)
            {
                (bool presetAvailable, bool stop) = terminationInternal.ShouldStop(agent.Name);
                if (presetAvailable)
                {
                    // Update hints with the final decision details.
                    streamingContent.Hints["terminate-decision"] = new Dictionary<string, string>
                    {
                        { "prompt", "preset decided" },
                        { "content", $"result for {agent.Name} is {stop}." },
                        { "reason", "code" }
                    };

                    // Yield the final termination decision.
                    yield return stop;
                    yield break;
                }
            }

            // STEP 1: Filter the conversation history using preconditions and a filter instruction.
            await foreach (var (promptText, filteredJson, filterThinking) in YamlHistory.GetFilteredHistoryResultAsync(
                preconditionPrompts: preconditionPromptsInternal,
                filterInstruction: filterInstructionInternal,
                chatHistory: chatHistory,
                kernel: kernelInstance,
                cancellationToken: cancellationToken))
            {
                filteredHistoryJson = filteredJson;

                // Update hints with the filtering step's results.
                streamingContent.Hints["terminate-history"] = new Dictionary<string, string>
                {
                    { "prompt", promptText },
                    { "content", filteredJson },
                    { "reason", filterThinking }
                };

                // Yield false as an intermediate result (termination not yet determined).
                yield return false;
            }

            // STEP 2: Build a prompt for evaluating whether the agent should terminate.
            string terminationPrompt = $@"
                You are a programmer. You have been provided with a JSON array of messages.

                **Instructions**:
                1. Evaluate the following statement based on the provided messages and description:
                ""{terminationDescriptionPromptInternal}""

                2. Return your answer as a JSON object with two properties:
                   - ""reason"": A string explaining why the statement is considered true or false.
                   - ""shouldTerminate"": A boolean value (true if the termination condition is met, false otherwise).

                These are the messages you need to evaluate:
                --------------------------------------------------
                ""{filteredHistoryJson}""
                --------------------------------------------------

                Example Output:
                {{
                    ""reason"": ""Your logic on the reason for your evaluation."",
                    ""shouldTerminate"": false
                }}

                Return only the JSON response without any additional commentary.
            ";

            string decisionJson = string.Empty;

            // STEP 3: Invoke the termination prompt and process the decision result.
            await foreach (var (finalPrompt, jsonResponse, decisionThinking) in YamlHelpers.GetDecisionAsync(terminationPrompt, filteredHistoryJson, kernelInstance, cancellationToken))
            {
                // Update hints with the termination decision prompt's results.
                streamingContent.Hints["terminate-content"] = new Dictionary<string, string>
                {
                    { "prompt", finalPrompt },
                    { "content", jsonResponse },
                    { "reason", decisionThinking }
                };

                // Yield false as an intermediate result (decision in progress).
                yield return false;
                decisionJson = jsonResponse;
            }

            // STEP 4: Parse the final JSON decision to extract the rationale and termination status.
            (string reasonText, bool shouldTerminate) = ExtractTerminationData(decisionJson);

            // Build the final rationale string.
            string finalRationale = shouldTerminate ? "True: " : "False: ";
            finalRationale += reasonText;

            // Update hints with the final decision details.
            streamingContent.Hints["terminate-decision"] = new Dictionary<string, string>
            {
                { "prompt", decisionJson },
                { "content", finalRationale },
                { "reason", "code" }
            };

            // Yield the final termination decision.
            yield return shouldTerminate;
        }

        /// <summary>
        /// Parses the JSON input to extract the termination rationale and decision.
        /// </summary>
        /// <param name="input">The JSON string containing the termination decision.</param>
        /// <returns>A tuple containing the rationale and a boolean indicating whether to terminate.</returns>
        public static (string reason, bool shouldTerminate) ExtractTerminationData(string input)
        {
            // Clean the JSON response.
            input = YamlHelpers.CleanJsonResponse(input);

            // Extract the termination rationale.
            string reason = YamlHelpers.ExtractValueByKey(input, "\"reason\":");

            // Extract the termination decision.
            bool shouldTerminate = YamlHelpers.ExtractBooleanByKey(input, "\"shouldTerminate\":");

            return (reason, shouldTerminate);
        }

        /// <summary>
        /// Synchronous termination decision is not implemented for this strategy.
        /// </summary>
        protected override Task<bool> ShouldAgentTerminateAsync(Agent agent, IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore SKEXP0110
