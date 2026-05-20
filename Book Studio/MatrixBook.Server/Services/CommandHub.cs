using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.AspNetCore.SignalR;


namespace MatrixBook.Server.Services;

public class CommandHub(ICommandDispatcher commandDispatcher) : Hub
{
    private readonly ICommandDispatcher _commandDispatcher = commandDispatcher;

    public async Task CommandFinished(CommandResult result)
    {
        await _commandDispatcher.Complete(result);
    }
}
