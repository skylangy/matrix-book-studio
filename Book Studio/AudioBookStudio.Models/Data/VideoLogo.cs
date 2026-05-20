namespace AudioBookStudio.Models.Data;
public class VideoLogo
{
    public MediaResource Image { get; set; }
    public string? Text { get; set; }
    public string? FontFamily { get; set; }
    public int FontSize { get; set; } = 36;
    public bool Shadow { get; set; }
}
