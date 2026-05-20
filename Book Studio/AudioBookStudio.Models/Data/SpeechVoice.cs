namespace AudioBookStudio.Models.Data;
public class SpeechVoice
{
    public required string Name { get; set; }
    public required string Value { get; set; }
    public string? Tag { get; set; }
}
