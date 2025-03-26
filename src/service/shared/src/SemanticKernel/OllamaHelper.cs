using Microsoft.SemanticKernel;
 
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

namespace MultiAgents.SemanticKernel
{
    /// <summary>
    /// Helper class for configuring and building an Ollama-based Semantic Kernel.
    /// It initializes the kernel with the Ollama Chat Completion service.
    /// </summary>
    public class OllamaHelper
    {
       

        /// <summary>
        /// Removes any text between <think> and </think> (including the tags) from the provided content.
        /// </summary>
        /// <param name="content">The content from which <think> tags will be removed.</param>
        /// <returns>The content with <think> tags and their content removed.</returns>
        public static string RemoveThinkContent(string content)
        {
            if (string.IsNullOrEmpty(content)) return content;

            const string startTag = "<think>";
            const string endTag = "</think>";

            int startIndex = content.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1) return content;

            int endIndex = content.IndexOf(endTag, startIndex, StringComparison.OrdinalIgnoreCase);
            if (endIndex >= 0)
            {
                // Extract parts before and after the <think> tag.
                string beforeThink = content.Substring(0, startIndex);
                string afterThink = content.Substring(endIndex + endTag.Length);
                return beforeThink + afterThink;
            }
            else
            {
                // If no closing </think> tag is found, remove content from <think> to the end.
                return content.Substring(0, startIndex);
            }
        }

        /// <summary>
        /// Splits the input text into plain text and "thinking" content (the content within <think> tags).
        /// </summary>
        /// <param name="input">The input content to be split.</param>
        /// <returns>A tuple containing the plain text and the "thinking" content.</returns>
        public static (string plainText, string thinkText) SplitContentFromThinking(string input)
        {
            const string startTag = "<think>";
            const string endTag = "</think>";

            int startIndex = input.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (startIndex == -1)
            {
                // No <think> tags found, return the input as plain text and an empty "think" section.
                return (input, string.Empty);
            }

            int endIndex = input.IndexOf(endTag, startIndex, StringComparison.OrdinalIgnoreCase);
            if (endIndex >= 0)
            {
                // Extract plain text and thinking content.
                string beforeThink = input.Substring(0, startIndex);
                string thinkSection = input.Substring(startIndex, endIndex - startIndex + endTag.Length);
                string afterThink = input.Substring(endIndex + endTag.Length);
                return (beforeThink + afterThink, thinkSection);
            }
            else
            {
                // If no closing </think> tag is found, return everything before <think> as plain text,
                // and the remaining content as thinking content.
                string beforeThink = input.Substring(0, startIndex);
                string thinkSection = input.Substring(startIndex);
                return (beforeThink, thinkSection);
            }
        }
 
    }
}
#pragma warning restore SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
