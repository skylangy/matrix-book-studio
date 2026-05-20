namespace AudioBookStudio.Common.Models;

public class ConvertResult
{
    public bool Success { get; set; }

    public Exception Exception { get; set; }

    public TimeSpan Duration { get; set; }
}