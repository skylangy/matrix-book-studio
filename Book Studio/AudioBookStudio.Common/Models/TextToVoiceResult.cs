namespace AudioBookStudio.Common.Models;

public sealed class TextToVoiceResult
{
    public bool Success { get; set; }
    public string OutputFile { get; set; }
    public string Error { get; set; }
}