namespace Matrix.Audio.Models;
public class EmailAttachment
{
    public required string Filename { get; set; }
    public required byte[] Content { get; set; }
    public string? ContentType { get; set; }
}
