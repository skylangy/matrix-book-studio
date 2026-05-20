namespace AudioBookStudio.Common.Models;
public class VideoCombineContext
{
    public string Name { get; set; }
    public string OutputFolder { get; set; }
    public string OutputFile { get; set; }
    public IEnumerable<string> SourceFiles { get; set; }
}
