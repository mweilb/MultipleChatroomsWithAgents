 
using Microsoft.SemanticKernel;
using MultiAgents.Agents.Strategies;
using YamlDotNet.Serialization;

namespace MultiAgents.Configurations
{

    // Room definition used in "chatrooms".
    public class YamlStrategyConfig
    {
        [YamlMember(Alias = "rules")]
        public List<YamlStratergyRules> Rules { get; set; } = [];

        // The decision associated with the rule.
        [YamlMember(Alias = "selection")]
        public YamlDecisionConfig? GlobalSelectAgentOrRoom { get; set; }

        // Termination rule.
        [YamlMember(Alias = "termination")]
        public YamlTerminationDecisionConfig? GlobalTermination { get; set; }

        public bool Setup(YamlRoomConfig room, Kernel kernel)
        {
            Rules?.RemoveAll(rule => rule == null);

            // If no rules exist, create one.
            if (Rules == null || Rules.Count == 0)
            {
                Rules =[new YamlStratergyRules()];
            }
            if (GlobalTermination != null)
            {
                if (string.IsNullOrEmpty(GlobalTermination.Name))
                {
                    GlobalTermination.Name = $"{room.Name} Termination"; 
                }
            }

            int ruleCount = 0;
            // Iterate through each rule.
            foreach (var rule in Rules)
            {
                ruleCount++;
                if (string.IsNullOrEmpty(rule.Name))
                {
                    rule.Name = $"Rule {ruleCount}";
                }

                    // If the rule does not have a selection configuration, assign the global one.
                if (rule.SelectAgentOrRoom == null && GlobalSelectAgentOrRoom != null)
                {
                    rule.SelectAgentOrRoom = GlobalSelectAgentOrRoom;
                }

                // If the rule does not have a termination configuration, assign the global one.
                if (rule.Termination == null && GlobalTermination != null)
                {
                    rule.Termination = GlobalTermination;

                }
                else if (rule.Termination != null)
                {
                    if (string.IsNullOrEmpty(rule.Termination.Name))
                    {
                        rule.Termination.Name = $"{rule.Name} Termination";
                    }
                }
                 

                    rule.TerminationStreamingStrategy = new YamlTerminationStrategy(kernel, rule.Termination);
                rule.SelectionStreamingStrategy = new YamlSelectionStrategy(kernel, rule.SelectAgentOrRoom, rule.Next);

                
            }
 
            return true;
        }



    }
}