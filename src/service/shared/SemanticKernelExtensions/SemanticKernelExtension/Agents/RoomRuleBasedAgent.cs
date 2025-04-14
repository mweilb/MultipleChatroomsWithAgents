using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.Orchestrator;
 
using System.Runtime.CompilerServices;
 
  

namespace SemanticKernelExtension.Agents
{
#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0110
    public class RoomRuleBasedAgent(
        string name,
        string agentName,
        string modelId,
        string message,
        bool visible,
        Kernel kernel,
        RuleBasedSettings? ruleBasedSettings
        ) : RoomAgent(name, agentName, modelId, message, visible, kernel, false)
    {
        private RuleBasedSettings? _ruleBasedSettings = ruleBasedSettings;
        private readonly RuleInfoManager _ruleInfoManager = new RuleInfoManager();

        /// <summary>
        /// Suggests whether the room should yield or not.
        /// Override this method to provide custom yield logic.
        /// </summary>
        /// <returns>True if the room should yield, otherwise false.</returns>
        public override bool ShouldYield(string roomName)
        {
            if (_ruleBasedSettings == null || _ruleBasedSettings.CurrentRule == null)
                return false;

            string ruleName = _ruleBasedSettings.CurrentRule.Name;
            if (_ruleInfoManager.TryGetYieldOnRoomChange(roomName, ruleName, out bool yieldOnRoomChange))
                return yieldOnRoomChange;
            return false;
        }
        /// <summary>
        /// Summarizes the chat history of the previous room and adds it as a system message in the new active chat room.
        /// </summary>
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>An asynchronous stream of <see cref="StreamingOrchestratorContent"/> representing the summary process.</returns>
        public override async IAsyncEnumerable<StreamingOrchestratorContent> RespondToRoomChange(string orchestratorName, string currentChatRoomName, AgentGroupChat currentChatRoom, AgentGroupChat lastChatRoom,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (currentChatRoom != lastChatRoom)
            {
                string instructions = "Summary the conversations";
                if (_ruleBasedSettings != null && _ruleBasedSettings.CurrentRule != null)
                {
                    string ruleName = _ruleBasedSettings.CurrentRule.Name;
                    if (_ruleInfoManager.TryGetInstructions(currentChatRoomName, ruleName, out string? ruleInstruction) && !string.IsNullOrEmpty(ruleInstruction))
                    {
                        instructions = ruleInstruction;
                    }

                this.setInstructions(instructions);

                await foreach (var agentChunk in SuccesfullChangedRoom(orchestratorName, currentChatRoomName, currentChatRoom, lastChatRoom, cancellationToken))
                {
                    yield return agentChunk;
                }
            }
            else
            {
                if (currentChatRoom != null)
                {
                    string returnAgentName = "System";
                    if (_ruleBasedSettings != null && _ruleBasedSettings.CurrentRule != null)
                    {
                        string ruleName = _ruleBasedSettings.CurrentRule.Name;
                        if (_ruleInfoManager.TryGetYieldCanceledName(currentChatRoomName, ruleName, out string? cancelName) && !string.IsNullOrEmpty(cancelName))
                        {
                            returnAgentName = cancelName;
                        }
                    }

                    currentChatRoom.AddChatMessage(new ChatMessageContent(AuthorRole.System, "User Canceled Room Change")
                    {
                        AuthorName = $"{returnAgentName}",
                    });
                }
            }
        }
        }

        public void SetExecutionSettings(RuleBasedSettings? ruleBasedSettings)
        {
            _ruleBasedSettings = ruleBasedSettings;
        }

        public void InsertRuleInfo(string room, string ruleName, string instructions, bool yieldOnChange, string yieldCanceledName)
        {
            _ruleInfoManager.InsertRuleInfo(room, ruleName, instructions, yieldOnChange, yieldCanceledName);
        }

        private readonly Kernel _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel), "Kernel is required to invoke the summary.");
    }
}
