using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using YamlConfigurations;

#pragma warning disable SKEXP0110
#pragma warning disable SKEXP0001

namespace AppExtensions.Experience.Factories
{
    /// <summary>
    /// Common helper methods to build prompt templates, kernel functions, and reducers
    /// for both KernelFunctionSelectionStrategy and KernelFunctionTerminationStrategy.
    /// </summary>
    public static class KernelFunctionStrategyCommon
    {
        /// <summary>
        /// Builds the prompt instructions (Step 1).
        /// </summary>
        /// <param name="instructions">The instructions from your YAML config.</param>
        /// <returns>A usable prompt template (or empty string if null).</returns>
        public static string BuildPromptTemplate(string? instructions)
        {
            return instructions ?? string.Empty;
        }

        /// <summary>
        /// Builds a KernelFunction from the given prompt template and history variable name (Step 2).
        /// </summary>
        /// <param name="promptTemplate">The prompt text to transform into a KernelFunction.</param>
        /// <param name="historyVariableName">The name of the history variable to use.</param>
        /// <returns>A KernelFunction for either selection or termination.</returns>
        public static KernelFunction BuildKernelFunction(
            string promptTemplate,
            string historyVariableName)
        {
            return AgentGroupChat.CreatePromptFunctionForStrategy(
                promptTemplate,
                safeParameterNames: historyVariableName
            );
        }

        /// <summary>
        /// Builds a chat history reducer (either summarization or truncation) (Step 3).
        /// </summary>
        /// <param name="summarizationReducer">An optional summarization reducer config.</param>
        /// <param name="truncationReducer">An optional truncation reducer config.</param>
        /// <param name="kernel">The kernel to use for building the reducers.</param>
        /// <returns>An IChatHistoryReducer or null if no config is provided.</returns>
        public static IChatHistoryReducer? BuildHistoryReducer(
            YamlSummarizationReducer? summarizationReducer,
            YamlTruncationReducer? truncationReducer,
            Kernel kernel)
        {
            // Summarization
            if (summarizationReducer is not null
                && int.TryParse(summarizationReducer.TargetCount, out int targetReducerCount))
            {
                if (int.TryParse(summarizationReducer.ThresholdCount, out int thresholdReducerCount))
                {
                    return new ChatHistorySummarizationReducer(
                        kernel.GetRequiredService<IChatCompletionService>(),
                        thresholdReducerCount,
                        targetReducerCount
                    );
                }
                else
                {
                    return new ChatHistorySummarizationReducer(
                        kernel.GetRequiredService<IChatCompletionService>(),
                        targetReducerCount
                    );
                }
            }

            // Truncation
            else if (truncationReducer is not null
                     && int.TryParse(truncationReducer.TargetCount, out int targetCount))
            {
                if (int.TryParse(truncationReducer.ThresholdCount, out int thresholdCount))
                {
                    return new ChatHistoryTruncationReducer(targetCount, thresholdCount);
                }
                else
                {
                    return new ChatHistoryTruncationReducer(targetCount);
                }
            }

            // Otherwise, no reducer
            return null;
        }

        /// <summary>
        /// A helper to see if the user input indicates "yes" or "true".
        /// </summary>
        public static bool IsTrue(string? value)
        {
            return value != null &&
                   (value.Contains("yes", StringComparison.OrdinalIgnoreCase) ||
                    value.Contains("true", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Builds a result parser that returns a <c>string</c> (for SelectionStrategy).
        /// This checks each pattern in the YAML expressions; if matched, returns the entire resultValue.
        /// Otherwise returns an empty string.
        /// </summary>
        public static Func<FunctionResult, string> BuildResultParser(YamlPromptSelect yamlPrompt)
        {
            // Default to false
            Func<FunctionResult, string> resultParser = (result) => result.GetValue<string>() ?? string.Empty;

            if (yamlPrompt.ResultParser?.Regex is { Count: > 0 } expressions)
            {
                resultParser = (fr) =>
                {
                    // Convert the function result to a string
                    string resultValue = fr.GetValue<string>() ?? string.Empty;

                    // Evaluate each YamlExpression in order
                    foreach (var expression in expressions)
                    {
                        // Skip invalid or empty pattern
                        if (string.IsNullOrEmpty(expression.Pattern)) continue;

                        // Check the pattern
                        var regex = new Regex(expression.Pattern, RegexOptions.IgnoreCase);
                        if (regex.IsMatch(resultValue))
                        {

                            return resultValue;
                        }
                    }
                    // If no patterns match, return false
                    return string.Empty;
                };
            }

            return resultParser;
        }

        /// <summary>
        /// Builds a result parser that returns a <c>bool</c> (for TerminationStrategy).
        /// This checks each pattern; if matched, uses the expression value to decide true/false.
        /// </summary>
        public static Func<FunctionResult, bool> BuildBoolResultParser(
            List<YamlExpression>? expressions)
        {
            // Default to false
            Func<FunctionResult, bool> defaultParser = (fr) =>
            {
                string resultValue = fr.GetValue<string>() ?? string.Empty;
                return IsTrue(resultValue);
            };
            
            if (expressions is { Count: > 0 })
            {
                return (fr) =>
                {
                    string resultValue = fr.GetValue<string>() ?? string.Empty;

                    foreach (var expr in expressions)
                    {
                        if (string.IsNullOrEmpty(expr.Pattern)) continue;

                        var regex = new Regex(expr.Pattern, RegexOptions.IgnoreCase);
                        if (regex.IsMatch(resultValue))
                        {
                            // If matched, interpret expr.Value as "yes" / "true"
                            return IsTrue(expr.Value);
                        }
                    }
                    return false;
                };
            }

            return defaultParser;
        }
    }
}
