using AppExtensions.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using SemanticKernelExtension.AgentGroupChats.Strategies.RuleBased;
using SemanticKernelExtension.AgentGroupChats.Strategies.Terminations;
using System.Reflection;
using YamlConfigurations;

#pragma warning disable SKEXP0110

namespace AppExtensions.Experience.Factories
{
    public class RuleBasedDefinitionsFactory
    {
        public static List<RuleBasedDefinition> Create(Kernel kernel, YamlRoomConfig roomConfig, List<ChatHistoryAgent> completionAgents)
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

                        ruleForSK.Name = rule.Name;
                        SetYieldOnRoomChange(rule, ruleForSK);

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

        private static void SetYieldOnRoomChange(YamlStratergyRules rule, RuleBasedDefinition ruleForSK)
        {
            ruleForSK.ShouldYield = rule.YieldOnRoomChange != null &&
                                   (rule.YieldOnRoomChange.Equals("yes", StringComparison.OrdinalIgnoreCase) || rule.YieldOnRoomChange.Equals("true", StringComparison.OrdinalIgnoreCase));
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

        private static void AddSelectionStrategy(YamlSelectionConfig? selection, RuleBasedDefinition ruleForSK, List<ChatHistoryAgent> agents, Kernel kernel)
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
                    ruleForSK.Selection = RoundRobinSelectionStrategyFactory.Create(selection.RoundRobinSelection, agents);

                    Console.WriteLine("        → Round-robin selection in this rule.");
                }
            }
            if (ruleForSK.Selection == null)
            {
                ruleForSK.Selection = new SequentialSelectionStrategy();
            }


            //HACK 

            var factory = kernel.Services.GetService<ILoggerFactory>();
            if (factory != null)
            {
                var logger = factory.CreateLogger(ruleForSK.Selection.GetType());
                if (logger != null)
                {
                    // Assuming 'strategy' is an instance of SelectionStrategy (or a derived type)
                    PropertyInfo? prop = typeof(SelectionStrategy).GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance);
                    prop?.SetValue(ruleForSK.Selection, logger);
                }
            }

        }

        private static void AddTerminationStrategy(YamlTerminationDecisionConfig? termination, RuleBasedDefinition ruleForSK, List<ChatHistoryAgent> agents, Kernel kernel)
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


            //Hack 
            var factory = kernel.Services.GetService<ILoggerFactory>();
            if (factory != null)
            {
                var logger = factory.CreateLogger(ruleForSK.Termination.GetType());
                if (logger != null)
                {
                    // Assuming 'strategy' is an instance of SelectionStrategy (or a derived type)
                    PropertyInfo? prop = typeof(TerminationStrategy).GetProperty("Logger", BindingFlags.NonPublic | BindingFlags.Instance);
                    prop?.SetValue(ruleForSK.Termination, logger);
                }
            }

        }
        
    }


}
