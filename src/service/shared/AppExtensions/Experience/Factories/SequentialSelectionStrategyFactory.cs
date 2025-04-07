 
 
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public static class SequentialSelectionStrategyFactory
    {
        public static SequentialSelectionStrategy Create(YamlSequentialSelection sequenceSelection, List<ChatHistoryAgent> agents)
        {
            var sequentialSelection = new SequentialSelectionStrategy();
            if (sequenceSelection.InitialAgent != null)
            {
                sequentialSelection.InitialAgent = agents.Find(
                            a => a.Name?.Equals(sequenceSelection.InitialAgent, StringComparison.OrdinalIgnoreCase) == true);
            }

            return sequentialSelection;
        }

    }


}
