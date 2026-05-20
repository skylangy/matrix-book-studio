namespace AudioBookStudio.Models.Data;
public class AzureSynthesizerConfig
{
    public string? VoiceName { get; set; }

    public string? Language { get; set; }

    public string VoiceFormat { get; set; } = ".wav";

    public int VoiceChunkSize { get; set; } = 2600;
    public bool UseSsml { get; set; } = true;

    public List<ServiceSubscription> Subscriptions { get; set; } = [];
}
