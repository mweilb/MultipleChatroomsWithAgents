

using MultiAgents.SemanticKernel.Modifications;
using YamlDotNet.Serialization;


namespace  MultiAgents.Configurations
{
    // A rule with its children promoted up (no nested agentFlowList).
    public class YamlStratergyRules
    {
        // The rule's name.
        [YamlMember(Alias = "name")]
        public string Name { get; set; } = string.Empty;

        // A list of agent references that are considered "current".
        [YamlMember(Alias = "current")]
        public List<YamlCurrentAgentConfig> Current { get; set; } = new List<YamlCurrentAgentConfig>();

        // The decision associated with the rule.
        [YamlMember(Alias = "selection")]
        public YamlDecisionConfig? SelectAgentOrRoom { get; set; }

        // Termination rule.
        [YamlMember(Alias = "termination")]
        public YamlTerminationDecisionConfig? Termination { get; set; }

        // A list of agent references for "next" steps.
        [YamlMember(Alias = "next")]
        public List<YamlNextAgentConfig> Next { get; set; } = new List<YamlNextAgentConfig>();

        //Implemenation details 

        // Strategy for terminating streaming interactions, if configured.
        public  TerminationStreamingStrategy? TerminationStreamingStrategy = null;

        // Strategy for selecting options during streaming interactions, if configured.
        public  SelectionStreamingStrategy? SelectionStreamingStrategy = null;

        
    }
}
