using MatrixBook.Tray.Models;
using Microsoft.Extensions.Logging;

namespace MatrixBook.Tray.Services;
public class QueueProcessingBackgroundService(
    ICommandQueue commandQueue,
    ICommandRunner commandRunner,
    ILogger<QueueProcessingBackgroundService> logger) : IBackgroundService
{
    private Task? _mainWorker;
    private bool _isRunning = false;
    private CancellationTokenSource? _cts;
    private readonly ICommandQueue _commandQueue = commandQueue;
    private readonly SemaphoreSlim _semaphore = new(5);
    private readonly List<Task> _runningTasks = [];
    private readonly ILogger<QueueProcessingBackgroundService> _logger = logger;
    private readonly ICommandRunner _commandRunner = commandRunner;

    public bool IsRunning => _isRunning;

    public void Start(CancellationToken cancellationToken)
    {
        try
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _mainWorker = Task.Run(MainLoopAsync, _cts.Token);
            _isRunning = true;
            _logger.LogInformation("Background service started successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start background service");
            _isRunning = false;
        }
    }

    public void Stop()
    {
        _cts?.Cancel();

        try
        {
            _mainWorker?.Wait();

            Task.WhenAll(_runningTasks).Wait(); // Wait for all in-flight tasks to finish

            _isRunning = false;
        }
        catch (AggregateException ex)
        {
            _logger.LogError(ex, "Error during shutdown of background service");
        }
    }

    private async Task MainLoopAsync()
    {
        var token = _cts!.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_commandQueue.TryDequeue() is CommandModel command)
                {
                    _logger.LogInformation("Start running command {command}", command.Command);
                    await _semaphore.WaitAsync(token);

                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            await ProcessCommandAsync(command, token);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    }, token);

                    lock (_runningTasks)
                    {
                        _runningTasks.Add(task);
                    }

                    _ = task.ContinueWith(t =>
                    {
                        lock (_runningTasks)
                        {
                            _runningTasks.Remove(t);
                        }
                    });
                }
                else
                {
                    await Task.Delay(500, token);
                }
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background processing loop");
            }
        }
    }

    private async Task ProcessCommandAsync(CommandModel command, CancellationToken token)
    {
        _logger.LogInformation("Processing command: {Command}", command);

        await _commandRunner.RunCommandAsync(command, token);
    }
}

