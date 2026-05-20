using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AudioBookStudio.Common.Services;
public class LocalCommandInvoker(ILogger<LocalCommandInvoker> logger) : ICommandInvoker
{
    private readonly ILogger<LocalCommandInvoker> _logger = logger;

    public async Task InvokeAsync(CommandModel command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = command.Command,
            Arguments = command.Arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger?.LogInformation("Output: {message}", e.Data);
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger?.LogInformation("Error: {message}", e.Data);
        }

        using var process = new Process
        {
            StartInfo = startInfo
        };
        process.OutputDataReceived += OutputHandler;
        process.ErrorDataReceived += ErrorHandler;

        process.Start();

        process.OutputDataReceived -= OutputHandler;
        process.ErrorDataReceived -= ErrorHandler;

        await process.WaitForExitAsync();
    }
}
