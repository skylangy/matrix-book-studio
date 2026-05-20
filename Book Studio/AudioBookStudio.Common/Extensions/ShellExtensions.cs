using AudioBookStudio.Common.Models;
using System.Diagnostics;

namespace AudioBookStudio.Common.Extensions;
public static class ShellExtensions
{
    public static async Task InvokeAsync(this CommandModel shellCommand, Action<string> log = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = shellCommand.Command,
            Arguments = shellCommand.Arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                log?.Invoke($"Output: {e.Data}");
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                log?.Invoke($"Error: {e.Data}");
        }

        var process = new Process
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
