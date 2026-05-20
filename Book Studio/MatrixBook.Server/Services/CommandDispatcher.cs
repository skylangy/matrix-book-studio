using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Text.Json;

namespace MatrixBook.Server.Services;

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IHubContext<CommandHub> _hubContext;
    private readonly ILogger<CommandDispatcher> _logger;
    private static readonly ConcurrentDictionary<string, TaskCompletionSource<CommandResult>> _pendingCommands = new();
    private static TimeSpan _commandTimeout = TimeSpan.FromSeconds(30);

    public CommandDispatcher(IHubContext<CommandHub> hubContext, ILogger<CommandDispatcher> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<CommandResult?> DispatchAsync(CommandModel command, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<CommandResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pendingCommands[command.Id] = tcs;


        var payload = JsonSerializer.Serialize(command);
        await _hubContext.Clients.All.SendAsync("ExecuteCommand", payload, cancellationToken);


        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(_commandTimeout, cancellationToken));

        _pendingCommands.TryRemove(command.Id, out _);

        if (completedTask == tcs.Task)
        {
            return await tcs.Task;
        }

        _logger.LogWarning("Command {Id} timed out after {Timeout} seconds.", command.Id, _commandTimeout.TotalSeconds);
        return null;
    }

    public Task Complete(CommandResult result)
    {
        if (_pendingCommands.TryGetValue(result.Id, out var tcs))
        {
            tcs.TrySetResult(result);
        }

        return Task.CompletedTask;
    }

}
