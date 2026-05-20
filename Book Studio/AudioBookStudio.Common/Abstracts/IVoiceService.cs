namespace AudioBookStudio.Common.Abstracts;
public interface IVoiceService
{
    IList<LanguageVoice> GetLanguages();
}
