
 
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using SemanticKernelExtension.AgentGroupChats.Strategies.Selections;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public static class RoundRobinSelectionStrategyFactory
    {
        public static RoundRobinSelectionStrategy? Create(YamlRoundRobinSelection sequenceSelection, List<ChatHistoryAgent> agents)
        {

            var validAgents = sequenceSelection.Agents != null
                            ? agents
                                .Where(a => a.Name is not null
                                            && sequenceSelection.Agents.Contains(a.Name, StringComparer.OrdinalIgnoreCase))
                                .ToList()
                            : [];


            var selection = new RoundRobinSelectionStrategy(validAgents);
            if (selection.InitialAgent != null)
            {
                selection.InitialAgent = agents.Find(
                            a => a.Name?.Equals(sequenceSelection.InitialAgent, StringComparison.OrdinalIgnoreCase) == true);
            }

            return selection;
        }

    }


}
