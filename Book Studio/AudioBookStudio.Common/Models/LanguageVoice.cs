namespace AudioBookStudio.Models;

public class LanguageVoice
{
    public NamedValue<string> Language { get; set; }

    public IList<NamedValue<string>> Voices { get; set; } = new List<NamedValue<string>>();
}
