 
using Microsoft.SemanticKernel;
using MultiAgents.SemanticKernel;
using System.Runtime.CompilerServices;
using System.Text;

#pragma warning disable SKEXP0001

namespace MultiAgents.Agents.Strategies
{
    /// <summary>
    /// Provides methods for filtering chat history and constructing a JSON array 
    /// from chat messages based on preconditions and a filtering prompt.
    /// </summary>
    public class YamlHistory
    {
        /// <summary>
        /// Processes chat history to apply filtering based on provided preconditions and a filter prompt.
        /// Constructs a JSON array of messages and sends a prompt to retrieve a filtered result.
        /// </summary>
        /// <param name="preconditionPrompts">Optional list of preconditions for processing history.</param>
        /// <param name="filterInstruction">Optional filtering instruction prompt.</param>
        /// <param name="chatHistory">The chat history as a list of messages.</param>
        /// <param name="kernel">The Semantic Kernel instance used for prompt invocation.</param>
        /// <param name="cancellationToken">Cancellation token for asynchronous streaming.</param>
        /// <returns>
        /// An asynchronous stream of tuples containing the prompt, filtered JSON result, and any thinking text.
        /// </returns>
        public static async IAsyncEnumerable<(string Prompt, string FilteredJson, string Thinking)> GetFilteredHistoryResultAsync(
            List<string>? preconditionPrompts,
            string? filterInstruction,
            IReadOnlyList<ChatMessageContent> chatHistory,
            Kernel kernel,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Create a mutable copy of the chat history.
            var filteredHistory = chatHistory.ToList();
            bool shouldRemoveContent = false;

            // Process each precondition prompt.
            if (preconditionPrompts != null)
            {
                foreach (var condition in preconditionPrompts)
                {
                    if (condition.Equals("last message", StringComparison.OrdinalIgnoreCase))
                    {
                        // If "last message" condition is met, retain only the last message.
                        if (chatHistory.Any())
                        {
                            filteredHistory = new List<ChatMessageContent> { chatHistory.Last() };
                        }
                    }
                    else if (condition.Equals("remove content", StringComparison.OrdinalIgnoreCase))
                    {
                        // Set flag to remove the message content.
                        shouldRemoveContent = true;
                    }
                }
            }

            // Create a JSON representation of the filtered chat history.
            var jsonItems = filteredHistory.Select((message, index) =>
            {
                // Escape any embedded quotes in the author name.
                var author = (message.AuthorName ?? "User").Replace("\"", "\\\"");
                // Clean the message content by removing any unwanted "thinking" parts and escape quotes.
                var messageText = OllamaHelper.RemoveThinkContent(message.Content ?? "").Replace("\"", "\\\"");
                
                // Build a JSON object string.
                if (shouldRemoveContent)
                {
                    return $"{{\"Order\":{index + 1},\"Name\":\"{author}\"}}";
                }
                return $"{{\"Order\":{index + 1},\"Name\":\"{author}\",\"Message\":\"{messageText}\"}}";
            });

            string jsonHistory = $"[{string.Join(",", jsonItems)}]";

            // If no filter instruction is provided, yield the JSON history with preconditions and exit.
            if (filterInstruction == null && preconditionPrompts != null)
            {
                string preconditionsJson = "[" + string.Join(",", preconditionPrompts.Select(p => $"\"{p}\"")) + "]";
                yield return ("code", jsonHistory, @$"applied pre-condition: {preconditionsJson}");
                yield break;
            }

            // Construct a detailed prompt using the filter instruction and the JSON history.
            string fullPrompt = "Task:\n" +
                "Given an array of messages, each with a Name and Content, process the array with the following logic:" +
                $@"{filterInstruction}\n\n" +
                "Remove any message that does not meet these criteria." +
                "Expected Output:\n" +
                "   1. Return a JSON array that contains only the last message, formatted as:\n" +
                "   2. No code, just the json array\n" +
                "   3. No explanation on how you found the answer, just the json array\n" +
                "Messages are below:\n" +
                "json\n" +
                "'''\n" +
                $@"{jsonHistory}\n" +
                "'''\n";

            // Invoke the prompt on the kernel and obtain a streaming response.
            var streamingResult = kernel.InvokePromptStreamingAsync(fullPrompt, null, null, null, cancellationToken);

            StringBuilder responseBuilder = new();
            // Process the streaming response chunks.
            await foreach (var chunk in streamingResult)
            {
                responseBuilder.Append(chunk.ToString() ?? string.Empty);
                // Split the response into the filtered JSON result and the associated "thinking" text.
                (var filteredJson, var thinkingText) = OllamaHelper.SplitContentFromThinking(responseBuilder.ToString());
                yield return (fullPrompt, filteredJson, thinkingText);
            }
        }
    }
}

#pragma warning restore SKEXP0001
