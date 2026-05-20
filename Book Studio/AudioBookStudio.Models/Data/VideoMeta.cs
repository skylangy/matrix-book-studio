namespace AudioBookStudio.Models.Data;
public class VideoMeta : Entity
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Content { get; set; }
    public string? BottomNote { get; set; }
    public string? Category { get; set; }
    public string? Tag { get; set; }
    public string? Status { get; set; }
    public string SpeechService { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public string VoiceName { get; set; } = string.Empty;
    public string? ContentFontFamily { get; set; }
    public int? ContentFontSize { get; set; }

    public VideoLogo Logo { get; set; }
    public MediaResource? IntroImage { get; set; }
    public MediaResource? OutroImage { get; set; }
    public List<MediaResource> ContentImages { get; set; } = [];
    public MediaResource? IntroAudio { get; set; }
    public double? Duration { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
