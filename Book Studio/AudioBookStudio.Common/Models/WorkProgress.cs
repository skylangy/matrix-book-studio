namespace AudioBookStudio.Common.Models;

public class WorkProgress : Entity
{
    public DateTime Timestamp { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public string Category { get; set; }
    public int Total { get; set; }
    public int Current { get; set; }
    public bool Success { get; set; } = true;
}