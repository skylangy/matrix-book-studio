using MatrixBook.Tray.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace MatrixBook.Tray.Services;
public class CommandQueue(ILogger<CommandQueue> logger) : ICommandQueue
{
    private readonly ConcurrentQueue<CommandModel> _queue = new();
    private readonly ILogger<CommandQueue> _logger = logger;

    public void TryEnqueue(CommandModel command)
    {
        ArgumentNullException.ThrowIfNull(command, nameof(command));
        _logger.LogInformation("Queue command: {command}", command.Command);
        _queue.Enqueue(command);
    }

    public CommandModel? TryDequeue()
    {
        _queue.TryDequeue(out var command);
        if (command != null)
            _logger.LogInformation("Dequeue command: {command}", command?.Command);
        return command;
    }

    public bool IsEmpty => _queue.IsEmpty;
}
