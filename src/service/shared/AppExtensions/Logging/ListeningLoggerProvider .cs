using System;
using Microsoft.Extensions.Logging;

namespace AppExtensions.Logging
{
    /// <summary>
    /// A custom ILoggerProvider that wraps all created loggers in a ListeningLogger.
    /// This way, regardless of where or when loggers are created, the events are intercepted.
    /// </summary>
    public class ListeningLoggerProvider : ILoggerProvider
    {
        private readonly ILoggerFactory _innerFactory;

        /// <summary>
        /// Initializes a new instance of the ListeningLoggerProvider.
        /// </summary>
        /// <param name="innerFactory">The underlying ILoggerFactory that creates the base loggers.</param>
        public ListeningLoggerProvider(ILoggerFactory innerFactory)
        {
            _innerFactory = innerFactory;
        }

        /// <summary>
        /// Creates a new ListeningLogger wrapping the logger created by the inner factory.
        /// </summary>
        /// <param name="categoryName">The category for the logger.</param>
        /// <returns>A ListeningLogger instance wrapping the base logger.</returns>
        public ILogger CreateLogger(string categoryName)
        {
            var baseLogger = _innerFactory.CreateLogger(categoryName);
            return new ListeningLogger(baseLogger);
        }

        public void Dispose()
        {
            // Dispose any resources if necessary.
        }
    }
}
