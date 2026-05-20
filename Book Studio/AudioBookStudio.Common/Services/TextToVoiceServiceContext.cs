namespace AudioBookStudio.Common.Services;

public class TextToVoiceServiceContext
{
    public required string SpeechService { get; set; }
    public string VoiceName { get; set; }

    public string Language { get; set; }

    public string OutputPath { get; set; }

    public string FileName { get; set; }

    public string VoiceFormat { get; set; }

    public string VoiceRole { get; set; }

    public string VoiceStyle { get; set; }

    public int VoiceChunkSize { get; set; }

    public bool UseSsml { get; set; } = true;

    public TextToVoiceServiceContext()
    {

    }

    public TextToVoiceServiceContext(TextToVoiceServiceContext context)
    {
        SpeechService = context.SpeechService;
        VoiceName = context.VoiceName;
        Language = context.Language;
        OutputPath = context.OutputPath;
        FileName = context.FileName;
        VoiceFormat = context.VoiceFormat;
        VoiceChunkSize = context.VoiceChunkSize;
        UseSsml = context.UseSsml;
    }

    //public TextToVoiceServiceContext(AppConfiguration config)
    //{
    //    //VoiceName = config.VoiceName;
    //    //Language = config.Language;
    //    //VoiceFormat = config.VoiceFormat;
    //    //VoiceChunkSize = config.VoiceChunkSize;
    //    //UseSsml = config.UseSsml;
    //}
}