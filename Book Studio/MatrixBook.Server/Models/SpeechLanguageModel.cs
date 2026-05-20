namespace MatrixBook.Server.Models;

public class SpeechLanguageModel
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public List<SpeechVoiceModel> Voices { get; set; } = new();
}