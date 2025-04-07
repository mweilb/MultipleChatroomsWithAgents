using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Runtime.CompilerServices;
 

namespace AppExtensions.Logging
{
    /// <summary>
    /// A decorator for IChatCompletionService which intercepts chat completion calls.
    /// It logs the calls and responses using external logging extensions.
    /// </summary>
    public class LearningInterceptor : IChatCompletionService
    {
        private readonly IChatCompletionService _innerService;
 

        // Optional event to allow external processing of intercepted messages.
        static public event Action<string, ChatMessageContent>? OnMessageCaptured;
        static public event Action<string, string, StreamingChatMessageContent>? OnMessageCapturedAsync;

        /// <summary>
        /// Constructs a LearningInterceptor by wrapping another IChatCompletionService and specifying an ILogger.
        /// </summary>
        /// <param name="innerService">The inner service to wrap.</param>
        /// <param name="logger">An ILogger for logging purposes.</param>
        public LearningInterceptor(IChatCompletionService innerService)
        {
            _innerService = innerService;
        }

        /// <summary>
        /// Intercepts a single chat completion call.
        /// </summary>
        public async Task<ChatMessageContent> GetChatMessageContentAsync(
            ChatHistory chat,
            PromptExecutionSettings settings,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {
             ChatMessageContent response = await _innerService.GetChatMessageContentAsync(chat, settings, kernel, cancellationToken);

             OnMessageCaptured?.Invoke("GetChatMessageContentAsync", response);

            return response;
        }

        /// <summary>
        /// Intercepts multiple chat completion calls.
        /// </summary>
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            CancellationToken cancellationToken = default)
        {

            var responses = await _innerService.GetChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken);
 
            foreach (var message in responses)
            {
                 OnMessageCaptured?.Invoke("GetChatMessageContentsAsync", message);
            }

            return responses;
        }

        /// <summary>
        /// Intercepts streaming chat completions.
        /// </summary>
        public async IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(
            ChatHistory chatHistory,
            PromptExecutionSettings? settings = null,
            Kernel? kernel = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {

            string id = Guid.NewGuid().ToString();
            await foreach (var chunk in _innerService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel, cancellationToken))
            {
                OnMessageCapturedAsync?.Invoke("GetStreamingChatMessageContentsAsync", id, chunk);
                yield return chunk;
            }
        }

        /// <summary>
        /// Returns attributes from the inner service.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Attributes => _innerService.Attributes;
    }
}
