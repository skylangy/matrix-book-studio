namespace AudioBookStudio.Films.Models;
public class FilmSettings
{
    public Size Resolution { get; set; } = new Size(1920, 1080);
    public double Duration { get; set; } = 0.0;
    public string BackgroundColor { get; set; } = "black";
    public int Framerate { get; set; } = 30;
    public string OutputPath { get; set; } = "output.mp4";
    public string PixelFormat { get; set; } = "yuv420p";
    public bool AllowTransparency { get; set; } = false;
    public bool Debug { get; set; } = false;
    public bool UseGpu { get; set; } = true;

    public string VideoCodec => UseGpu ? "h264_nvenc" : "libx264";
    public string AudioCodec => "aac";
    public string Output => OutputPath;

    public static FilmSettings ForYouTubeShort(string output)
         => new FilmSettings { Resolution = new Size(1080, 1920), OutputPath = output };
}