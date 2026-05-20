namespace MatrixBook.Server.Models;

public class SpeechServiceConfigModel
{
    public required string Name { get; set; }
    public required string Language { get; set; }
    public required string VoiceName { get; set; }
    public List<SpeechLanguageModel> Languages { get; set; } = [];
}