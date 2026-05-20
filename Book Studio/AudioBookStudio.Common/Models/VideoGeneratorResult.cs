namespace AudioBookStudio.Common.Models;

public class VideoGeneratorResult
{
    public bool Success { get; set; }
    public string OutputFile { get; set; }
    public string Error { get; set; }

    public TimeSpan Duration { get; set; }
}