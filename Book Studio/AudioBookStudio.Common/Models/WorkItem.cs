namespace AudioBookStudio.Common.Models;

public enum WorkStatus
{
    Created,
    Enqueued,
    Dequeued,
    Executing,
    Completed
}

public class WorkItem
{
    public WorkItem(Func<CancellationToken, Task> task)
    {
        Task = task ?? throw new ArgumentNullException(nameof(task));
        Status = WorkStatus.Created;
    }

    public Func<CancellationToken, Task> Task { get; }

    public WorkStatus Status { get; set; }

    public string Name { get; set; }

    public string Group { get; set; }

    public Action CompleteAction { get; set; }

    public void Complete()
    {
        Status = WorkStatus.Completed;
        CompleteAction?.Invoke();
    }
}