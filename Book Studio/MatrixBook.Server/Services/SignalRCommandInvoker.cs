using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;

namespace MatrixBook.Server.Services;

public class SignalRCommandInvoker(
    ICommandDispatcher commandDispatcher,
    ILogger<SignalRCommandInvoker> logger) : ICommandInvoker
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;
    private readonly ILogger<SignalRCommandInvoker> _logger = logger;

    public async Task InvokeAsync(CommandModel command)
    {
        _logger.LogInformation("Invoking command: {Id} {Command} {Arguments}", command.Id, command.Command, command.Arguments);

        await _commandDispatcher.DispatchAsync(command);
    }
}
