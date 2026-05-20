
namespace AudioBookStudio.Common.Models;

#nullable enable

public class ExportModel
{
    public string? BookName { get; set; }
    public string? Author { get; set; }
    public string? Type { get; set; }
    public string? SpeechService { get; set; }
    public string? Language { get; set; }
    public string? VoiceName { get; set; }
    public string? Image { get; set; }
    public List<Chapter> Chapters { get; set; } = [];
}
