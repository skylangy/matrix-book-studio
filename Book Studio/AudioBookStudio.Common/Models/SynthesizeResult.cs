namespace AudioBookStudio.Common.Models;
public class SynthesizeResult
{
    public bool Success { get; set; }
    public string Reason { get; set; }
    public string Error { get; set; }
    public string OutputFile { get; set; }
}
