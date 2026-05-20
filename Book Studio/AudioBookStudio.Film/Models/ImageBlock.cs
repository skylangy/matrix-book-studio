namespace AudioBookStudio.Films.Models;
public class ImageBlock
{
    public required string ImagePath { get; set; }
    public string Transition { get; set; } = "fade";
    public double FadeInDuration { get; set; } = 0;
    public double TransitionDuration { get; set; } = 0;
    public double Duration { get; set; }
}
