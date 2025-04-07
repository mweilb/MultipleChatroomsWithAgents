using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.ChatCompletion;
using System;

namespace AppExtensions.Logging
{
    public static class LearningInterceptorLoggerExtensions
    {
        // Log a chat completion request with chat history details.
        private static readonly Action<ILogger, string, string, Exception?> s_logChatCompletionRequest =
            LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(30, "LogChatCompletionRequest"),
                formatString: "[{Method}] Chat completion request initiated with chat history: {ChatHistory}");

        // Log a chat completion response with content.
        private static readonly Action<ILogger, string, string, Exception?> s_logChatCompletionResponse =
            LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(31, "LogChatCompletionResponse"),
                formatString: "[{Method}] Chat completion response: {Content}");

        // Log an individual chat message during a batch call.
        private static readonly Action<ILogger, string, string, Exception?> s_logChatMessage =
            LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(32, "LogChatMessage"),
                formatString: "[{Method}] Chat message: {Content}");

        // Log the initiation of a streaming chat request.
        private static readonly Action<ILogger, string, Exception?> s_logChatStreamingRequest =
            LoggerMessage.Define<string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(33, "LogChatStreamingRequest"),
                formatString: "[{Method}] Streaming chat completion request initiated");

        // Log a streaming chat message chunk.
        private static readonly Action<ILogger, string, string, Exception?> s_logChatStreamingChunk =
            LoggerMessage.Define<string, string>(
                logLevel: LogLevel.Information,
                eventId: new EventId(34, "LogChatStreamingChunk"),
                formatString: "[{Method}] Streaming chat chunk received: {Content}");

        public static void LogChatCompletionRequest(this ILogger logger, string method, ChatHistory chatHistory)
        {
            // You can change the string conversion as needed (e.g., by serializing specific properties).
            s_logChatCompletionRequest(logger, method, chatHistory.ToString() ?? "", null);
        }

        public static void LogChatCompletionResponse(this ILogger logger, string method, string content)
        {
            s_logChatCompletionResponse(logger, method, content, null);
        }

        public static void LogChatMessage(this ILogger logger, string method, string content)
        {
            s_logChatMessage(logger, method, content, null);
        }

        public static void LogChatStreamingRequest(this ILogger logger, string method)
        {
            s_logChatStreamingRequest(logger, method, null);
        }

        public static void LogChatStreamingChunk(this ILogger logger, string method, string content)
        {
            s_logChatStreamingChunk(logger, method, content, null);
        }

    }
}
