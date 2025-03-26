using Microsoft.SemanticKernel;
using System.Runtime.CompilerServices;
using System.Text;
 
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using MultiAgents.SemanticKernel;

#pragma warning disable SKEXP0001

namespace MultiAgents.Agents.Strategies
{
    /// <summary>
    /// Provides helper methods for processing and extracting data from JSON responses,
    /// as well as interacting with the kernel to get streaming responses.
    /// </summary>
    public static class YamlHelpers
    {
        /// <summary>
        /// Cleans the JSON response by removing backticks and any leading "json" identifier.
        /// Also extracts the valid JSON object from the response string.
        /// </summary>
        /// <param name="response">The raw response string to clean.</param>
        /// <returns>A cleaned JSON string.</returns>
        public static string CleanJsonResponse(string response)
        {
            // Remove leading and trailing backticks and extra spaces.
            string cleanedResponse = response.TrimStart('`').TrimEnd('`').Trim();

            // Remove a leading "json" identifier if present.
            if (cleanedResponse.StartsWith("json", StringComparison.OrdinalIgnoreCase))
            {
                cleanedResponse = cleanedResponse.Substring(4).Trim();
            }

            // Locate the first '{' and the last '}' to extract the JSON object.
            int jsonStartIndex = cleanedResponse.IndexOf('{');
            int jsonEndIndex = cleanedResponse.LastIndexOf('}');

            if (jsonStartIndex >= 0 && jsonEndIndex >= jsonStartIndex)
            {
                cleanedResponse = cleanedResponse.Substring(jsonStartIndex, jsonEndIndex - jsonStartIndex + 1);
            }

            return cleanedResponse;
        }

        /// <summary>
        /// Constructs and sends a prompt to get a JSON decision based on the provided prompt.
        /// The method streams the response and yields a tuple containing the original prompt,
        /// the parsed result, and any associated thinking text.
        /// </summary>
        /// <param name="prompt">The prompt to send to the kernel.</param>
        /// <param name="kernel">The kernel instance used to process the prompt.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>An asynchronous enumerable of tuples (prompt, result, thinking).</returns>
        public static async IAsyncEnumerable<(string Prompt, string Result, string Thinking)> GetDecisionAsync(
            string prompt,
            string messages,
            Kernel kernel,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            KernelArguments arguments = new() { { "query", prompt },{ "messages", messages } };
            HandlebarsPromptTemplateFactory promptTemplateFactory = new();

                // Invoke the prompt asynchronously using the kernel's streaming API.
            var streamingResponse = kernel.InvokePromptStreamingAsync(prompt, arguments,
                    templateFormat: HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
                    promptTemplateFactory: promptTemplateFactory,
                    cancellationToken: cancellationToken);

            StringBuilder responseBuilder = new();

            // Yield parts of the streaming response as they arrive.
            await foreach (var chunk in streamingResponse)
            {
                responseBuilder.Append(chunk.ToString() ?? string.Empty);
                
                // Split the current response into the result and any thinking text.
                (var result, var thinking) = OllamaHelper.SplitContentFromThinking(responseBuilder.ToString());

                yield return (prompt, result, thinking);
            }
        }

        /// <summary>
        /// Extracts a string value associated with the specified key from the input JSON-like string.
        /// </summary>
        /// <param name="input">The input string containing key-value pairs.</param>
        /// <param name="key">The key whose associated value is to be extracted.</param>
        /// <returns>The extracted string value, or an empty string if the key is not found or not valid.</returns>
        public static string ExtractValueByKey(string input, string key)
        {
            int keyStartIndex = input.IndexOf(key);

            if (keyStartIndex == -1)
            {
                // Key not found, return empty string.
                return string.Empty;
            }

            // Find the starting index of the value by locating the first quote after the key.
            int valueStartIndex = input.IndexOf("\"", keyStartIndex + key.Length);
            int valueEndIndex = input.IndexOf("\"", valueStartIndex + 1);

            if (valueStartIndex == -1 || valueEndIndex == -1)
            {
                // Invalid value format, return empty string.
                return string.Empty;
            }

            // Extract and return the value between the quotes.
            return input.Substring(valueStartIndex + 1, valueEndIndex - valueStartIndex - 1).Trim();
        }

        /// <summary>
        /// Extracts a boolean value associated with the specified key from the input JSON-like string.
        /// </summary>
        /// <param name="input">The input string containing key-value pairs.</param>
        /// <param name="key">The key whose associated boolean value is to be extracted.</param>
        /// <returns>The extracted boolean value, or false if the key is not found or parsing fails.</returns>
        public static bool ExtractBooleanByKey(string input, string key)
        {
            int keyStartIndex = input.IndexOf(key);

            if (keyStartIndex == -1)
            {
                // Key not found, default to false.
                return false;
            }

            // Extract the substring starting after the key.
            string boolValue = input.Substring(keyStartIndex + key.Length).Trim();

            // Determine the end of the value by checking for a comma or closing brace.
            int valueEndIndex = boolValue.IndexOf(",");
            if (valueEndIndex != -1)
            {
                boolValue = boolValue.Substring(0, valueEndIndex).Trim();
            }
            else
            {
                valueEndIndex = boolValue.IndexOf("}");
                if (valueEndIndex != -1)
                {
                    boolValue = boolValue.Substring(0, valueEndIndex).Trim();
                }
            }

            // Attempt to parse the extracted substring as a boolean.
            bool.TryParse(boolValue, out bool parsedResult);
            return parsedResult;
        }
    }
}

#pragma warning restore SKEXP0001
