namespace AudioBookStudio.Common.Models;

public class TaskDescriptor
{
    public required string Name { get; set; }

    public required string Group { get; set; }

    public Func<CancellationTokenSource, Task> TaskProvider { get; set; }
}