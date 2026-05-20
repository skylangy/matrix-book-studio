using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace AudioBookStudio.Common.Extensions;
public static class TaskExtensions
{
    public static ConfiguredTaskAwaitable DefaultAwait(this Task task)
    {
        return task.ConfigureAwait(false);
    }

    public static ConfiguredTaskAwaitable<T> DefaultAwait<T>(this Task<T> task)
    {
        return task.ConfigureAwait(false);
    }

    public static IBackgroundWorkerQueue EnqueueJob(this IBackgroundWorkerQueue backgroundWorkerQueue, JobDescriptor jobDescriptor, CancellationTokenSource cancellationTokenSource)
    {
        var workItems = new List<WorkItem>();
        if (jobDescriptor.PrepareTask is not null)
        {
            workItems.Add(new WorkItem(async token =>
            {
                await jobDescriptor.PrepareTask.TaskProvider(cancellationTokenSource);
            })
            {
                Name = jobDescriptor.PrepareTask.Name,
                Group = jobDescriptor.Name,
                Status = WorkStatus.Enqueued
            });
        }

        if (jobDescriptor.MainTasks is not null)
        {
            foreach (var task in jobDescriptor.MainTasks)
            {
                workItems.Add(new WorkItem(async token =>
                {
                    await task.TaskProvider(cancellationTokenSource);
                })
                {
                    Name = task.Name,
                    Group = jobDescriptor.Name,
                    Status = WorkStatus.Enqueued
                });
            }
        }


        if (jobDescriptor.DoneTask is not null)
        {
            workItems.Add(new WorkItem(async token =>
            {
                await jobDescriptor.DoneTask.TaskProvider(cancellationTokenSource);
            })
            {
                Name = jobDescriptor.DoneTask.Name,
                Group = jobDescriptor.Name,
                Status = WorkStatus.Enqueued
            });
        }

        var workItemGroup = new WorkItemGroup(jobDescriptor.Name, workItems);
        backgroundWorkerQueue.Enqueue(workItemGroup);

        return backgroundWorkerQueue;
    }

    public static IBackgroundWorkerQueue EnqueueJob(this IBackgroundWorkerQueue backgroundWorkerQueue, ConcurrentQueue<JobDescriptor> jobDescriptors, CancellationTokenSource cancellationTokenSource)
    {
        while (jobDescriptors.TryDequeue(out var jobDescriptor))
        {
            backgroundWorkerQueue.EnqueueJob(jobDescriptor, cancellationTokenSource);
        }
        return backgroundWorkerQueue;
    }

    public static async Task<T> Retry<T>(this Task<T> task, int maxRetries = 8, int retryIntervalMs = 5000)
    {
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                retries++;

                await Task.Delay(retryIntervalMs).ConfigureAwait(false);
            }
        }

        return await task.ConfigureAwait(false);
    }

    public static async Task Retry(this Task task, int maxRetries = 8, int retryIntervalMs = 5000)
    {
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch (Exception)
            {
                retries++;

                await Task.Delay(retryIntervalMs).ConfigureAwait(false);
            }
        }

        await task.ConfigureAwait(false);
    }

    public static async Task Retry(Action action, int maxRetries = 3, int retryIntervalMs = 500)
    {
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                retries++;

                await Task.Delay(retryIntervalMs).ConfigureAwait(false);
            }
        }
    }

}
