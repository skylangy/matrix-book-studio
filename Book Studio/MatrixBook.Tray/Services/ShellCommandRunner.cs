using MatrixBook.Tray.Common;
using MatrixBook.Tray.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace MatrixBook.Tray.Services;
public class ShellCommandRunner(
    IOptions<Configuration> configuration,
    ICommandHistory commandHistory,
    ILogger<ShellCommandRunner> logger) : ICommandRunner
{
    private readonly Configuration _configuration = configuration.Value;
    private readonly ICommandHistory _commandHistory = commandHistory;
    private readonly ILogger<ShellCommandRunner> _logger = logger;

    public async Task RunCommandAsync(CommandModel command, CancellationToken token)
    {
        _logger.LogInformation("Starting command: {Command} {Arguments}", command.Command, command.Arguments);

        Func<Task> func = () => Task.Run(() => RunCommand(command, token));
        await func.Retry(exceptionHandler: (ex) =>
        {
            _logger.LogError(ex, "Run command with exception {message}", ex.Message);
        });
    }

    private async Task RunCommand(CommandModel command, CancellationToken token)
    {
        if (token.IsCancellationRequested)
        {
            _logger.LogInformation("Command execution cancelled.");
            return;
        }

        var argument = ProcessArgument(command.Arguments);
        _logger.LogInformation("Updated arguments: {Command} {args}", command.Command, argument);
        var historyItem = new CommandHistoryItem
        {
            Command = command.Command,
            Arguments = argument
        };
        _commandHistory.Add(historyItem);

        var startInfo = new ProcessStartInfo
        {
            FileName = command.Command,
            Arguments = argument,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        using var process = new Process { StartInfo = startInfo };
        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger.LogInformation("Output: {Data}", e.Data);
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger.LogError("Run '{Command}' with Error: {Data}", command.Command, e.Data);
        }
        process.OutputDataReceived += OutputHandler;
        process.ErrorDataReceived += ErrorHandler;
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.OutputDataReceived -= OutputHandler;
        process.ErrorDataReceived -= ErrorHandler;

        _logger.LogInformation("Process started with ID: {ProcessId}", process.Id);


        await process.WaitForExitAsync(token);

        _logger.LogInformation("Command completed successfully {processId} {command}.", process.Id, command.Command);
    }

    private string ProcessArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
        {
            return string.Empty;
        }

        foreach (var mapp in _configuration.PathMapping)
        {
            if (argument.StartsWith(mapp.Key, StringComparison.OrdinalIgnoreCase)
                || argument.Contains(mapp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return argument.Replace(mapp.Key, mapp.Value, StringComparison.OrdinalIgnoreCase).ConvertToWindowsPath();
            }
        }
        return argument;
    }
}