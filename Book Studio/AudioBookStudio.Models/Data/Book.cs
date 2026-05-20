

namespace AudioBookStudio.Models.Data;

public class Book : Entity
{
    public required string Title { get; set; }
    public string? Subtitle { get; set; }
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string? Author { get; set; }
    public string? AuthorId { get; set; }
    public string? Status { get; set; }
    public List<string> CategoryIds { get; set; } = [];
    public List<string> TagIds { get; set; } = [];
    public string? Year { get; set; }
    public string? DefaultImageId { get; set; }
    public string? Language { get; set; }
    public string? VoiceName { get; set; }
    public string? SpeechService { get; set; } = "Edge";

    public bool WavGenerated { get; set; }
    public bool Mp3Generated { get; set; }
    public bool Mp4Generated { get; set; }
    public bool SrtGenerated { get; set; }
    public bool TextGenerated { get; set; }
    public bool Hide { get; set; }

    public float PublishOrder { get; set; } = 10000;
    public float Rank { get; set; }
    public long TextCount { get; set; }
    public DateTime? DateUpdated { get; set; }
    public DateTime? DateCreated { get; set; }
    public List<string> ImageIds { get; set; } = [];
}