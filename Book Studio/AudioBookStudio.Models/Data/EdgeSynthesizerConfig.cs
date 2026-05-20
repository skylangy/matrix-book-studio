namespace AudioBookStudio.Models.Data;

public class EdgeSynthesizerConfig
{
    public required string Endpoint { get; set; }
    public required string VoiceName { get; set; } = "zh-CN-YunjianNeural";
    public required string Language { get; set; } = "zh-CN";
}
