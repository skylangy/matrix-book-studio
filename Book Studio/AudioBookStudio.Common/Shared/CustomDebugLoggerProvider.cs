#nullable enable

using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AudioBookStudio.Common.Shared;
public class CustomDebugLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new CustomDebugLogger(categoryName);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private class CustomDebugLogger : ILogger
    {
        private readonly string _shortCategory;

        public CustomDebugLogger(string categoryName)
        {
            _shortCategory = categoryName.Split('.').Last();
        }

        IDisposable? ILogger.BeginScope<TState>(TState state)
        {
            return default;
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (string.IsNullOrWhiteSpace(message))
                return;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            Debug.WriteLine($"[{timestamp}] [{_shortCategory}] [{logLevel}]: {message}");
        }
    }
}
