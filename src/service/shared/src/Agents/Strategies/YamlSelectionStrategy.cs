 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using MultiAgents.Configurations;
using MultiAgents.SemanticKernel.Modifications;
using System.Runtime.CompilerServices;

#pragma warning disable SKEXP0110

namespace MultiAgents.Agents.Strategies
{
    /// <summary>
    /// Implements a selection strategy that uses YAML configuration to decide which agent should respond next.
    /// The strategy processes conversation history using preconditions and filtering, then applies a selection prompt.
    /// </summary>
    public class YamlSelectionStrategy(Kernel kernel, YamlDecisionConfig? decision, List<YamlNextAgentConfig> next)
        : SelectionStreamingStrategy
    {

        private readonly YamlDecisionConfig? selectionsInternal = decision;
        private readonly List<YamlNextAgentConfig> nextChoices = next;

        // Instance of the kernel used to invoke prompts.
        private readonly Kernel kernelInstance = kernel ?? throw new ArgumentNullException(nameof(kernel));
     

        public string NextName { get; set; } = "";
        public string ContentTransfer { get; internal set; } = "";

        /// <summary>
        /// Streams the agent selection process by first filtering the conversation history and then applying a selection prompt.
        /// Intermediate results are yielded as null until the final agent is selected.
        /// </summary>
        /// <param name="streamingContent">Container for hints and intermediate results.</param>
        /// <param name="agents">List of available agents.</param>
        /// <param name="chatHistory">The conversation history.</param>
        /// <param name="cancellationToken">Token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous stream that eventually yields the selected agent.</returns>
        public override async IAsyncEnumerable<Agent?> SelectAgentStreaming(
            AgentStreamingContent streamingContent,
            IReadOnlyList<Agent> agents,
            IReadOnlyList<ChatMessageContent> chatHistory,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            string filteredHistoryJson = string.Empty;

            //defaults than the current rule can override
            string detailPrompt = "";
            List<string> agentNameArray = [.. agents.Select(a => a.Name)];
            string defaultAgentName = agents[0].Name ?? "";
            List<string> preconditionPrompts = [];
            string filterInstruction = "";
            
            //provide the rule to the next agent
            if (selectionsInternal != null){
                detailPrompt = selectionsInternal.Prompt;
                preconditionPrompts = selectionsInternal.MessagePresetFilters;
                filterInstruction = selectionsInternal.MessagesFilter;
            }
     
            if (nextChoices != null && nextChoices.Count > 0 && !nextChoices.Any(a => a.Name == "any")){
                agentNameArray = [.. nextChoices.Select(a => a.Name)];
                defaultAgentName = nextChoices[0].Name ?? "";
            }    
            

            string agentNames = string.Join(", ", agentNameArray);

            if (agentNameArray.Count == 1)
            {

                streamingContent.Hints["select-decision"] = new Dictionary<string, string>
                {
                    { "prompt", "code" },
                    { "content", "selcted agent " +  agentNameArray[0] },
                    { "reason", "only one agent" }
                };

                NextName = agentNameArray[0];
            }
            else {

                // STEP 1: Filter the conversation history based on preconditions and filter instructions.
                await foreach (var (promptText, filteredJson, filterThinking) in YamlHistory.GetFilteredHistoryResultAsync(
                    preconditionPrompts: preconditionPrompts,
                    filterInstruction: filterInstruction,
                    chatHistory: chatHistory,
                    kernel: kernelInstance,
                    cancellationToken: cancellationToken))
                {
                    // Update hints with the intermediate filtering result.
                    streamingContent.Hints["select-history"] = new Dictionary<string, string>
                    {
                        { "prompt", promptText },
                        { "content", filteredJson },
                        { "reason", filterThinking }
                    };

                    // Yield null to indicate ongoing processing.
                    yield return null;

                    // Save the filtered history JSON for the next step.
                    filteredHistoryJson = filteredJson;
                }

                // STEP 2: Construct the selection prompt using the filtered history and the selection criteria.
                string selectionPrompt = $@"
                    History:
                    {filteredHistoryJson}

                    Agent Selection Criteria:
                    {detailPrompt}

                    The valid agents are:
                    {agentNames}

                    Based on the above, please return a JSON object with two properties:
                    - ""rationale"": An explanation of your decision.
                    - ""nextAgent"": The name of the agent to respond next.

                    Example Output:
                    {{
                        ""rationale"": ""Your reasoning why you selected this agent."",
                        ""nextAgent"": ""{defaultAgentName}""
                    }}

                    Return only the JSON response without any additional commentary.
                ";

                string decisionJson = string.Empty;

                // STEP 3: Invoke the selection prompt and process the decision result.
                await foreach (var (finalPrompt, decisionResult, decisionThinking) in YamlHelpers.GetDecisionAsync(selectionPrompt, filteredHistoryJson, kernelInstance, cancellationToken))
                {
                    // Update hints with the intermediate decision result.
                    streamingContent.Hints["select-content"] = new Dictionary<string, string>
                    {
                        { "prompt", finalPrompt },
                        { "content", decisionResult },
                        { "reason", decisionThinking }
                    };

                    // Yield null to indicate processing is still underway.
                    yield return null;

                    // Save the decision JSON for final parsing.
                    decisionJson = decisionResult;
                    
                }

                // STEP 4: Parse the final JSON decision to extract the next agent's name and rationale.
                (NextName, string rationale) = ExtractSelectionData(decisionJson);

                // Update final hints with the parsed decision details.
                streamingContent.Hints["select-decision"] = new Dictionary<string, string>
                {
                    { "prompt", decisionJson },
                    { "content", rationale },
                    { "reason", "code" }
                };
            }
            // Select the agent by matching the name; fallback to the first agent if no match is found.
            var selectedAgent = agents.FirstOrDefault(a => string.Equals(a.Name, NextName, StringComparison.OrdinalIgnoreCase));

            //assuming the null selected agent means a room chat was selected, GetTransferContent will validate that
            if (selectedAgent == null)
            {
                await foreach(var transfer in GetTransferContent(NextName, nextChoices, streamingContent, chatHistory,cancellationToken))
                {
                    yield return null;
                }
            }   

            // Yield the final selected agent.
            yield return selectedAgent;
        }

        private async IAsyncEnumerable<bool> GetTransferContent(string nextName, List<YamlNextAgentConfig>? next, AgentStreamingContent streamingContent, IReadOnlyList<ChatMessageContent> chatHistory, [EnumeratorCancellation]  CancellationToken cancellationToken)
        {
            if (next != null && next.Count > 0)
            {
                
                var nextAgent = next.FirstOrDefault(a => a.Name == nextName);
                if (nextAgent != null && nextAgent.ContextTransfer != null)
                {
                    var decision = nextAgent.ContextTransfer;
                    string filteredHistoryJson = string.Empty;

                    (bool presetFound, bool answer) = nextAgent.ContextTransfer.EvaluatePresets("skip");
                    if (presetFound == true && answer == true)
                    {
                        streamingContent.RequestedChatRoomContext = "preset skipped";
                        streamingContent.RequestChatRoomChange = true;
                        streamingContent.RequestedChatRoom = nextName;

                        yield return true;
                        yield break;
                    }


                    // STEP 1: Filter the conversation history based on preconditions and filter instructions.
                    await foreach (var (promptText, filteredJson, filterThinking) in YamlHistory.GetFilteredHistoryResultAsync(
                        preconditionPrompts: decision.MessagePresetFilters ?? [],
                        filterInstruction: decision.MessagesFilter ?? "",
                        chatHistory: chatHistory,
                        kernel: kernelInstance,
                        cancellationToken: cancellationToken))
                    {
                        // Update hints with the intermediate filtering result.
                        streamingContent.Hints["transfer-history"] = new Dictionary<string, string>
                        {
                            { "prompt", promptText },
                            { "content", filteredJson },
                            { "reason", filterThinking }
                        };

                        // Yield null to indicate ongoing processing.
                        yield return false;

                        // Save the filtered history JSON for the next step.
                        filteredHistoryJson = filteredJson;
                    }

                    // STEP 2: Invoke the selection prompt and process the decision result.
                    await foreach (var (finalPrompt, decisionResult, decisionThinking) in YamlHelpers.GetDecisionAsync(decision.Prompt, filteredHistoryJson, kernelInstance, cancellationToken))
                    {
                        // Update hints with the intermediate decision result.
                        streamingContent.Hints["transfer-content"] = new Dictionary<string, string>
                        {
                            { "prompt", finalPrompt },
                            { "content", decisionResult },
                            { "reason", decisionThinking }
                        };

                        // Yield null to indicate processing is still underway.
                        yield return false;

                        // Save the decision JSON for final parsing.
                        streamingContent.RequestedChatRoomContext = decisionResult;
                    }
                }
            }
            
            streamingContent.RequestChatRoomChange = true;
            streamingContent.RequestedChatRoom = nextName;

            yield return true;

        }



        /// <summary>
        /// Extracts the selected agent's name and decision rationale from the JSON response.
        /// </summary>
        /// <param name="input">The JSON string containing the selection decision.</param>
        /// <returns>A tuple with the next agent's name and the rationale behind the decision.</returns>
        public static (string nextAgent, string rationale) ExtractSelectionData(string input)
        {
            // Clean up the JSON response to ensure valid formatting.
            input = YamlHelpers.CleanJsonResponse(input);

            // Extract the next agent's name.
            string nextAgent = YamlHelpers.ExtractValueByKey(input, "\"nextAgent\":");

            // Extract the rationale for the selection.
            string rationale = YamlHelpers.ExtractValueByKey(input, "\"rationale\":");

            return (nextAgent, rationale);
        }

        /// <summary>
        /// Synchronous agent selection is not implemented for this strategy.
        /// </summary>
        protected override Task<Agent> SelectAgentAsync(IReadOnlyList<Agent> agents, IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}

#pragma warning restore SKEXP0110
