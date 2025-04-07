using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using Microsoft.SemanticKernel.ChatCompletion;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public static class KernelFunctionTerminationStrategyFactory
    {
        /// <summary>
        /// Creates a KernelFunctionTerminationStrategy from a YAML prompt configuration
        /// and a list of ChatHistoryKernelAgents.
        /// </summary>
        /// <param name="yamlPrompt">The YAML-based configuration for termination.</param>
        /// <param name="agents">The list of ChatHistoryKernelAgent to consider for termination.</param>
        /// <param name="kernel">The Kernel to use for creating semantic functions.</param>
        /// <returns>A fully-configured KernelFunctionTerminationStrategy, or null if no instructions are provided.</returns>
        public static KernelFunctionTerminationStrategy? Create(
            YamlPromptTermination yamlPrompt,
            List<ChatHistoryAgent> agents,
            Kernel kernel)
        {
            // Step 1: Build the prompt template
            var promptTemplate = KernelFunctionStrategyCommon.BuildPromptTemplate(yamlPrompt.Instructions);
            if (string.IsNullOrEmpty(promptTemplate))
            {
                return null;
            }

            // Step 2: Build the kernel function
            var historyVarName = yamlPrompt.HistoryVariableName
                                 ?? KernelFunctionTerminationStrategy.DefaultHistoryVariableName;
            var terminationFunction = KernelFunctionStrategyCommon.BuildKernelFunction(promptTemplate, historyVarName);

            // Step 3: Build the history reducer
            var historyReducer = KernelFunctionStrategyCommon.BuildHistoryReducer(
                yamlPrompt.SummarizationReducer,
                yamlPrompt.TruncationReducer,
                kernel
            );

            // Step 4: Build the bool-based result parser
            var resultParser = KernelFunctionStrategyCommon.BuildBoolResultParser(yamlPrompt.ResultParser?.Regex);

            // Step 5: EvaluateNameOnly
            bool evaluateNameOnly = KernelFunctionStrategyCommon.IsTrue(yamlPrompt.EvaluateNameOnly);

            // Step 6: Build the termination strategy
            var strategy = new KernelFunctionTerminationStrategy(terminationFunction, kernel)
            {
                HistoryVariableName = historyVarName,
                ResultParser = resultParser,
                HistoryReducer = historyReducer,
                EvaluateNameOnly = evaluateNameOnly
            };

            // Step 7. Filter valid agents
            if (yamlPrompt.Agents is { Count: > 0 })
            {
                var validAgents = agents
                    .Where(a => a.Name is not null &&
                                yamlPrompt.Agents.Contains(a.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                strategy.Agents = validAgents;
            }

            return strategy;
        }
    }
}
