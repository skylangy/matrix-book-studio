using System.Collections.Concurrent;

namespace AudioBookStudio.Common.Models;
public class JobDescriptor
{
    public string Name { get; set; }

    public ConcurrentQueue<TaskDescriptor> MainTasks { get; set; }

    public TaskDescriptor DoneTask { get; set; }

    public TaskDescriptor PrepareTask { get; set; }
}
