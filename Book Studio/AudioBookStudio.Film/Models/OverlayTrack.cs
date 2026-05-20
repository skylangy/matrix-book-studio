namespace AudioBookStudio.Films.Models;
public abstract class OverlayTrack : Track
{
    public OverlayTrack()
    {
        IsOverlay = true;
    }
}
