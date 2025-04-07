using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using YamlConfigurations;
using Microsoft.SemanticKernel.Agents.Chat;
using SemanticKernelExtension.AgentGroupChats.Strategies.Terminations;
 

#pragma warning disable SKEXP0110

namespace AppExtensions.Experience.Factories
{
    public class RuleBasedDefinitionsFactory
    {
        public static List<RuleBasedDefinition> Create(Kernel kernel, YamlRoomConfig roomConfig, List<ChatHistoryKernelAgent> completionAgents)
        {
            var listSKRules = new List<RuleBasedDefinition>();


            // Check strategies for this room
            if (roomConfig.Strategies != null)
            {
                var strategies = roomConfig.Strategies;

                // Now print out each rule
                if (strategies.Rules != null && strategies.Rules.Any())
                {
                    Console.WriteLine("  Rules:");
                    int index = 0;
                    foreach (var rule in strategies.Rules)
                    {
                         RuleBasedDefinition ruleForSK = new();

                        index++;
                        Console.WriteLine($"    [{index}] Rule Name: {rule.Name}");

                        AddCurrentAgents(rule, ruleForSK);
                        AddNextAgents(rule, ruleForSK);
                        AddSelectionStrategy(rule.Selection, ruleForSK, completionAgents, kernel);
                        AddTerminationStrategy(rule.Termination, ruleForSK, completionAgents, kernel);

                        listSKRules.Add(ruleForSK);
                    }
                }
                else
                {
                    Console.WriteLine("  (No rules found in this room.)");
                }
            }
            else
            {
                Console.WriteLine("  (No strategies found for this room.)");
            }

            return listSKRules;
        }

        private static void AddCurrentAgents(YamlStratergyRules rule, SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased.RuleBasedDefinition ruleForSK)
        {
            // Current
            if (rule.Current != null && rule.Current.Any())
            {
                Console.WriteLine("      Current Agents:");
                foreach (var c in rule.Current)
                {
                    ruleForSK.CurrentAgentNames.Add(c.Name);
                    Console.WriteLine($"        - {c.Name}");
                }
            }
        }

        private static void AddNextAgents(YamlStratergyRules rule, SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased.RuleBasedDefinition ruleForSK)
        {
            // Next
            if (rule.Next != null && rule.Next.Any())
            {
                Console.WriteLine("      Next Agents:");
                foreach (var n in rule.Next)
                {
                    ruleForSK.NextAgentsNames.Add(n.Name);
                    Console.WriteLine($"        - {n.Name}");
                }
            }
        }

        private static void AddSelectionStrategy(YamlSelectionConfig? selection, RuleBasedDefinition ruleForSK, List<ChatHistoryKernelAgent> agents, Kernel kernel)
        {
            // Selection
            if (selection != null)
            {
                Console.WriteLine("      Selection: (found)");
                if (selection.PromptSelect != null)
                {
                    ruleForSK.Selection = KernelFunctionSelectionStrategyFactory.Create(selection.PromptSelect, agents, kernel);
                }
                else if (selection.SequentialSelection != null)
                {
                    ruleForSK.Selection = SequentialSelectionStrategyFactory.Create(selection.SequentialSelection, agents);
                }
                else if (selection.RoundRobinSelection != null)
                {
                    Console.WriteLine("        → Round-robin selection in this rule.");
                }
            }
            if (ruleForSK.Selection == null)
            {
                ruleForSK.Selection = new SequentialSelectionStrategy();
            }


        }

        private static void AddTerminationStrategy(YamlTerminationDecisionConfig? termination, RuleBasedDefinition ruleForSK, List<ChatHistoryKernelAgent> agents, Kernel kernel)
        {
            // Termination
            if (termination != null)
            {
                ruleForSK.ContinuationAgentName = termination.ContinuationAgentName ?? "";

                if (termination.PromptTermination != null)
                {
                    ruleForSK.Termination = KernelFunctionTerminationStrategyFactory.Create(termination.PromptTermination, agents, kernel);
                }
                if (termination.RegexTermination != null)
                {
                    var patterns = termination.RegexTermination.Patterns ?? [];
                    ruleForSK.Termination = new RegexTerminationStrategy(patterns.ToArray());
                }
                else if (termination.ConstantTermination != null)
                {
                    ruleForSK.Termination = ConstantTerminationStrategyFactory.Create(termination.ConstantTermination, agents);
                }
            }
            
            ruleForSK.Termination ??= new ConstantTerminationStrategy(false);
            
        }
    }


}
