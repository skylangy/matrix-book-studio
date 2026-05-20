namespace AudioBookStudio.Models.Data;
public class SpeechLanguage
{
    public required string Name { get; set; }

    public required string Value { get; set; }

    public List<SpeechVoice> Voices { get; set; } = [];
}
