using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernelExtension.ChatCompletionService
{
    /// <summary>
    /// Intercepts LLM responses and extracts <think> blocks.
    /// Attaches them to message metadata for UI or logging.
    /// </summary>
    public class ThinkingInterceptor(IChatCompletionService inner) : IChatCompletionService
    {
        private readonly IChatCompletionService _inner = inner;

        /// <summary>
        /// Intercepts full response and extracts <think>...</think> block.
        /// Attaches the extracted thought to metadata.
        /// </summary>
        public async Task<ChatMessageContent> GetChatMessageContentAsync(
            ChatHistory chat,
            PromptExecutionSettings settings,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _inner.GetChatMessageContentAsync(chat, settings, kernel, cancellationToken);
            var match = Regex.Match(response.Content ?? "", @"<think>(.*?)</think>", RegexOptions.Singleline);
            if (!match.Success) return response;

            var thought = match.Groups[1].Value.Trim();
            var main = Regex.Replace(response.Content!, @"<think>.*?</think>", "", RegexOptions.Singleline).Trim();

            return new ChatMessageContent(AuthorRole.Assistant, main, response.ModelId, metadata: new Dictionary<string, object?> { ["think"] = thought });
        }

        /// <summary>
        /// Pass-through for multiple messages. No custom logic.
        /// </summary>
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
            var response = await _inner.GetChatMessageContentAsync(chatHistory, settings, kernel, cancellationToken);

            var content = response.Content ?? string.Empty;
            var match = Regex.Match(content, @"<think>(.*?)</think>", RegexOptions.Singleline);

            string thought = match.Success ? match.Groups[1].Value.Trim() : string.Empty;
            string main = match.Success ? Regex.Replace(content, @"<think>.*?</think>", "", RegexOptions.Singleline).Trim() : content.Trim();

            var message = new ChatMessageContent(
                AuthorRole.Assistant,
                main,
                response.ModelId,
                metadata: new Dictionary<string, object?> { ["think"] = thought }
            );

            return new List<ChatMessageContent> { message };
        }
        /// <summary>
        /// Intercepts streaming responses and attaches <think> block to metadata once complete.
        /// </summary>
        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
           
            bool captureThinking = false;
            string thinkingChunk;
            string responseChunk;
            await foreach (var chunk in _inner.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
            {
                thinkingChunk = "";
                responseChunk = "";
                if (chunk.Content is not null)
                {        
                    if (chunk.Content.StartsWith("<think>"))
                    {
                        captureThinking = true;
                    }
                    
                    int index = chunk.Content.IndexOf("</think>");
                    if (index >= 0 && captureThinking)
                    {
                        captureThinking = false;
                        thinkingChunk = chunk.Content.Substring(0, index);
                        responseChunk = chunk.Content.Substring(index, chunk.Content.Length - "</think>".Length);
                    }
                    else if (captureThinking)
                    {
                        thinkingChunk = chunk.Content;
                    }
                    else
                    {
                        responseChunk = chunk.Content;
                    }

                }
                

                // Attach thought to all subsequent chunks as metadata
                var metadata = captureThinking
                        ? new Dictionary<string, object?> { ["think"] = thinkingChunk }
                        : new Dictionary<string, object?> { ["think"] = "" };

                yield return new StreamingChatMessageContent(chunk.Role, responseChunk, metadata:metadata)
                {
                    Encoding = chunk.Encoding
                };
            }
        }

        /// <summary>
        /// Returns model attributes (e.g., ModelId, MaxTokens, etc.).
        /// </summary>
        public IReadOnlyDictionary<string, object?> Attributes => _inner.Attributes;
    }
}
