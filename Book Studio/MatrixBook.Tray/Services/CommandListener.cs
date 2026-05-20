using Microsoft.Extensions.Logging;


namespace MatrixBook.Tray.Services;

public class CommandListener(
    ICommandQueue commandQueue,
    ICommandRunner commandRunner,
    IMessageClient messageClient,
    ILogger<CommandListener> logger) : ICommandListener
{
    private readonly ILogger<CommandListener> _logger = logger;
    private readonly ICommandQueue _commandQueue = commandQueue;
    private readonly ICommandRunner _commandRunner = commandRunner;
    private readonly IMessageClient _messageClient = messageClient;
    private readonly HashSet<string> _queueCommands = ["ffmpeg"];

    public async Task Start()
    {
        try
        {
            _messageClient.AddExecuteCommandHandler(async (command) =>
            {
                _logger.LogInformation("Received command: {Name}", command.Command);

                if (_queueCommands.Contains(command.Command))
                {
                    _logger.LogInformation("Command {Name} is queued for later execution.", command.Command);
                    _commandQueue.TryEnqueue(command);
                }
                else
                {
                    _logger.LogInformation("Executing command {Name} immediately.", command.Command);
                    var cancellationToken = new CancellationTokenSource().Token;
                    await _commandRunner.RunCommandAsync(command, cancellationToken);
                }
            });

            await _messageClient.StartAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Connect to Server with error: {Error}", ex.Message);
        }
    }
}
