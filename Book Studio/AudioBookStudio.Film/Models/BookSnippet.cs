namespace AudioBookStudio.Films.Models;

public class BookSnippet
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required string Subtitle { get; set; }
    public required string Content { get; set; }
    public required string Audio { get; set; }
    public required string BgImagePath { get; set; }
    public required string IntroImagePath { get; set; }
    public required string OutroImagePath { get; set; }
    public required string IntroAudioPath { get; set; }
    public string BottomNote { get; set; } = string.Empty;
    public string? ContentFont { get; set; } = null;
    public int? ContentFontSize { get; set; } = null;

    public int? Width { get; set; }
    public int? Height { get; set; }

    public string? MetaTitle { get; set; }
    public string? MetaArtist { get; set; }

    public static BookSnippet ForDay(DateTime dateTime,
        string audio,
        string introAudio,
        string bgImage,
        string introImage,
        string outroImage,
        string title,
        string subtitle,
        string content,
        string bottomNote = "",
        string? bodyFont = null,
        int? contentFontSize = null)
    {
        var date = dateTime.ToString("yyyy-MM-dd");

        return new()
        {
            Name = date,
            Audio = audio,
            IntroAudioPath = introAudio,
            BgImagePath = bgImage,
            IntroImagePath = introImage,
            OutroImagePath = outroImage,
            Subtitle = subtitle,
            Title = title,
            Content = content,
            BottomNote = bottomNote,
            ContentFont = bodyFont,
            ContentFontSize = contentFontSize
        };
    }
}