 
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.Chat;
using System.Text.RegularExpressions;
using YamlConfigurations;

namespace AppExtensions.Experience.Factories
{
#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

    public static class KernelFunctionSelectionStrategyFactory
    {
        /// <summary>
        /// Creates a KernelFunctionSelectionStrategy from a YAML prompt configuration
        /// and a list of ChatHistoryKernelAgents.
        /// </summary>
        /// <param name="yamlPrompt">The YAML-based configuration for selection.</param>
        /// <param name="agents">The list of ChatHistoryKernelAgent to consider for selection.</param>
        /// <param name="kernel">The Kernel to use for creating semantic functions.</param>
        /// <returns>A fully-configured KernelFunctionSelectionStrategy, or null if no instructions are provided.</returns>
        public static KernelFunctionSelectionStrategy? Create(
            YamlPromptSelect yamlPrompt,
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
            var selectionFunction = KernelFunctionStrategyCommon.BuildKernelFunction(promptTemplate, historyVarName);

            // Step 3: Build the history reducer
            var historyReducer = KernelFunctionStrategyCommon.BuildHistoryReducer(
                yamlPrompt.SummarizationReducer,
                yamlPrompt.TruncationReducer,
                kernel
            );

            // Step 4: Build the string-based result parser
            var resultParser = KernelFunctionStrategyCommon.BuildResultParser(yamlPrompt);

            // Step 5: EvaluateNameOnly
            bool evaluateNameOnly = KernelFunctionStrategyCommon.IsTrue(yamlPrompt.EvaluateNameOnly);

            // Step 6: Build the selection strategy
            var strategy = new KernelFunctionSelectionStrategy(selectionFunction, kernel)
            {
                HistoryVariableName = historyVarName,
                HistoryReducer = historyReducer,
                ResultParser = resultParser,
                EvaluateNameOnly = evaluateNameOnly
            };

            // (If you wanted to filter Agents here, do it as needed, but typically that logic
            //  is more for Termination strategies.)

            return strategy;
        }
      
    }
}
