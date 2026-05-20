namespace Matrix.Audio.Models;
public class EmailOptions
{
    public required List<string> To { get; set; }
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public string? From { get; set; }
    public List<string>? Cc { get; set; }
    public List<string>? Bcc { get; set; }
    public List<EmailAttachment>? Attachments { get; set; }
}

