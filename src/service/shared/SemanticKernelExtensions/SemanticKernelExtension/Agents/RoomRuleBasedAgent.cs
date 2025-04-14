using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.Orchestrator;
 
using System.Runtime.CompilerServices;
using System.Text;
 

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
        RuleBasedSettings? ruleBasedSettings = null,
        string summarizeInstructions = "Summarize the Conversations as concise as possible and dont try to answer questions."
        ) : RoomAgent(name, agentName, modelId, message, visible, kernel, false, summarizeInstructions)
    {
        private RuleBasedSettings ? _ruleBasedSettings = ruleBasedSettings;

        /// <summary>
        /// Suggests whether the room should yield or not.
        /// Override this method to provide custom yield logic.
        /// </summary>
        /// <returns>True if the room should yield, otherwise false.</returns>
        public override bool ShouldYield()
        {
            return (_ruleBasedSettings?.ShouldYield() ?? false);
        }

        public void SetExecutionSettings(RuleBasedSettings? ruleBasedSettings)
        {
            _ruleBasedSettings = ruleBasedSettings;
        }

        private readonly Kernel _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel), "Kernel is required to invoke the summary.");
    }
}
