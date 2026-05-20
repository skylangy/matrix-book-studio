namespace Matrix.Audio.Models;

public class AppConfiguration
{
    public required string AppName { get; set; }
    public required string AppVersion { get; set; }

    public required string ConfigFolder { get; set; }
    public required string BooksLocation { get; set; }
    public required string DbUrl { get; set; }
    public required string DbName { get; set; }

    public required string BaseUrl { get; set; }

}
