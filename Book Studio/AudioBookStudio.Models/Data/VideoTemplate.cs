namespace AudioBookStudio.Models.Data;

public record Rect(double X, double Y, double Width, double Height);

public record Location(double X, double Y);

public record Size(double Width, double Height);

public record Margin(double Top, double Right, double Bottom, double Left);

public class Transition
{
    public string Effect { get; set; } = "fade";
    public double Duration { get; set; } = 0.5;
}

public class Sizes
{
    public static Size YouTubeHD => new(1920, 1080);
    public static Size YouTubeShort => new(1080, 1920);
}

public class Locations
{
    public static Location TopLeft => new(0, 0);
    public static Location TopCenter => new(0.5, 0);
    public static Location TopRight => new(1, 0);
    public static Location MiddleLeft => new(0, 0.5);
    public static Location Center => new(0.5, 0.5);
    public static Location MiddleRight => new(1, 0.5);
    public static Location BottomLeft => new(0, 1);
    public static Location BottomCenter => new(0.5, 1);
    public static Location BottomRight => new(1, 1);
}

public class FontNames
{
    public static string Arial => "Arial";
    public static string YaHei => "Microsoft YaHei";
}

public class Weights
{
    public static string Normal => "Normal";
    public static string Bold => "Bold";
    public static string Light => "Light";
}

public class Colors
{
    public static string White { get; set; } = "#FFFFFF";
    public static string Black { get; set; } = "#000000";
}

public class Margins
{
    public static Margin None => new(0, 0, 0, 0);
    public static Margin Small => new(5, 5, 5, 5);
    public static Margin Medium => new(10, 10, 10, 10);
    public static Margin Large => new(20, 20, 20, 20);
}

public class VideoTemplate : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public Size Resolution { get; set; } = Sizes.YouTubeHD;
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public List<VideoTrack> Tracks { get; set; } = [];
    public List<TemplateProperty> Properties { get; set; } = [];
}