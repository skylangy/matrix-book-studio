using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services;

public class BackgroundWorkerService(
    IBackgroundWorkerQueue backgroundWorkerQueue,
    ILogger<BackgroundWorkerService> logger) : BackgroundService
{
    private const int MaxParallelism = 5;
    private readonly SemaphoreSlim _concurrencySemaphore = new(MaxParallelism);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundWorkerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItemGroup = await backgroundWorkerQueue.DequeueAsync(stoppingToken);

            if (workItemGroup is null)
            {
                await Task.Delay(1000, stoppingToken); // Wait before checking again
                continue;
            }

            logger.LogInformation("Processing WorkItemGroup: {GroupName}", workItemGroup.Name);

            // Create a list to track all tasks in the group
            var groupTasks = new List<Task>();

            // Process all items in the group
            while (workItemGroup.TryDequeue(out var workItem))
            {
                // Wait for an available slot in the semaphore
                await _concurrencySemaphore.WaitAsync(stoppingToken);

                // Create a task for the work item
                var task = Task.Run(async () =>
                {
                    try
                    {
                        logger.LogInformation("Starting WorkItem: {WorkItemName} in group: {GroupName}", workItem.Name ?? "Unnamed", workItemGroup.Name);

                        workItem.Status = WorkStatus.Dequeued;
                        await workItem.Task(stoppingToken);


                        logger.LogInformation("Completed WorkItem: {WorkItemName} in group: {GroupName}", workItem.Name ?? "Unnamed", workItemGroup.Name);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error processing WorkItem: {WorkItemName} in group: {GroupName}", workItem.Name ?? "Unnamed", workItemGroup.Name);
                    }
                    finally
                    {
                        workItem.Complete();
                        _concurrencySemaphore.Release();
                    }
                }, stoppingToken);

                groupTasks.Add(task);
            }

            // Wait for all tasks in the group to complete before moving to the next group
            await Task.WhenAll(groupTasks);

            logger.LogInformation("Completed WorkItemGroup: {GroupName}", workItemGroup.Name);
        }

        logger.LogInformation("BackgroundWorkerService is stopping.");
    }
}
