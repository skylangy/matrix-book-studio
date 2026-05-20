using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface ISpeechSynthesizer
{
    Task<SynthesizeResult> SynthesizeAsync(string content, SynthesizeContext context);

    string VoiceMapping(string voiceName, string language);
}
