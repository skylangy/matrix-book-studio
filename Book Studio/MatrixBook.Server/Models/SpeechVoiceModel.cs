namespace MatrixBook.Server.Models;

public class SpeechVoiceModel
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public string? Tag { get; set; }
}