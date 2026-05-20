namespace AudioBookStudio.Common.Models;
public class SynthesizeContext
{
    public string Language { get; set; }
    public string VoiceName { get; set; }
    public string OutputPath { get; set; }
    public string FileName { get; set; }
    public bool UseSsml { get; set; }
}
