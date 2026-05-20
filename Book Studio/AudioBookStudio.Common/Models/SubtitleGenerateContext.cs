namespace AudioBookStudio.Common.Models;
public class SubtitleGenerateContext
{
    public Chapter Chapter { get; set; }
    public string AudioFilePath { get; set; }
    public string SubtitleFilePath { get; set; }
    public string AudioFolder { get; set; }
    public string SrtFolder { get; set; }
}
