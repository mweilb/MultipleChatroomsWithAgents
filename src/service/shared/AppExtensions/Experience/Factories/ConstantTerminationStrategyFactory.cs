 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using SemanticKernelExtension.AgentGroupChats.Strategies.Terminations;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public static class ConstantTerminationStrategyFactory
    {
        public static ConstantTerminationStrategy Create(YamlConstantTermination constantTermination, List<ChatHistoryAgent> agents)
        {
            string constantString = constantTermination.Value ?? "";
            bool contanstValue = constantString.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                                 constantString.Contains("true", StringComparison.OrdinalIgnoreCase);
            var constantStrategy = new ConstantTerminationStrategy(contanstValue);
            if (constantTermination.Agents is { Count: > 0 })
            {
                var validAgents = agents
                    .Where(a => a.Name is not null &&
                                constantTermination.Agents.Contains(a.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                constantStrategy.Agents = validAgents;
            }

            return constantStrategy;
        }
    }

}

