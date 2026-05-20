namespace AudioBookStudio.Common.Models;
public class ScriptResult
{
    public bool Success { get; set; }
    public string Output { get; set; }
    public string Error { get; set; }
    public int ExitCode { get; set; }
}
