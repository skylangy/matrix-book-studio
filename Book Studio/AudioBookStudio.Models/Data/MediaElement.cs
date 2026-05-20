namespace AudioBookStudio.Models.Data;


public class LayoutOptions
{
    public Location Location { get; set; } = Locations.TopLeft;
    public Size Size { get; set; } = Sizes.YouTubeHD;
    public Align Align { get; set; }
    public Margin Margin { get; set; } = Margins.None;
}

public class FontOptions
{
    public string Family { get; set; } = FontNames.YaHei;
    public double FontSize { get; set; } = 24;
    public string Weight { get; set; } = Weights.Normal;
    public string Color { get; set; } = Colors.White;
    public string BackgroundColor { get; set; } = Colors.Black;
    public bool? Shadow { get; set; }
}

public enum Align
{
    Top,
    Center,
    Bottom,
    Left,
    Right,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
    MiddleLeft,
    MiddleRight
}

public enum DurationType
{
    Fixed,
    Auto,
    Full
}

public enum StartType
{
    Absolute,
    AfterPrevious,
    BeforeEnd
}

public enum MediaType
{
    Audio,
    Image,
    Video,
    Text,
    Combine
}


public class MediaElement
{
    public required string Type { get; set; }
    public required string Name { get; set; }
    public double Start { get; set; }
    public double Duration { get; set; }
    public double FadeIn { get; set; }
    public double FadeOut { get; set; }
    public double Alpha { get; set; }
    public int? ZIndex { get; set; }
    public Transition Transition { get; set; } = new();
    public LayoutOptions Layout { get; set; } = new();
    public FontOptions Font { get; set; } = new();
    public string? DurationType { get; set; }
    public string? StartType { get; set; }

    public string? InputSource { get; set; }

    public string? TextContent { get; set; }

    public List<MediaElement> Children { get; set; } = [];
}
