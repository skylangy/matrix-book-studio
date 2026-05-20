namespace AudioBookStudio.Models.Data;


public class SpeechServiceConfig
{
    public required string Name { get; set; }

    public List<SpeechLanguage> Languages { get; set; } = [];
    public List<SpeechSubscription> Subscriptions { get; set; } = [];
    public string VoiceRegion { get; set; } = "westus";
    public string VoiceFormat { get; set; } = ".wav";
    public string Language { get; set; } = "zh-CN";
    public string VoiceName { get; set; } = "zh-CN-YunzeNeural";
    public string? EndPoint { get; set; }
    public bool UseSsml { get; set; } = true;
    public int VoiceChunkSize { get; set; } = 2600;
}
