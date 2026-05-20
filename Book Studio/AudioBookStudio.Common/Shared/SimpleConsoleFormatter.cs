#nullable enable
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace AudioBookStudio.Common.Shared;

public class SimpleConsoleFormatter : ConsoleFormatter
{
    public SimpleConsoleFormatter() : base("short") { }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var categoryName = logEntry.Category.Split('.').Last();
        var logLevel = logEntry.LogLevel;
        var message = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        textWriter.WriteLine($"[{logLevel}] {categoryName}: {message}");
    }
}
