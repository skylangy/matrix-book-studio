namespace AudioBookStudio.Models.Data;
public class AppConfiguration
{
    public required string BookDatabasePath { get; set; } = string.Empty;

    public required string OptionDatabasePath { get; set; } = string.Empty;

    public required string RegexLibDatabasePath { get; set; } = string.Empty;

    public required string Synthesizer { get; set; } = "Azure";

    public string? LogDatabasePath { get; set; }

    public required string BooksLocation { get; set; } = string.Empty;

    public int PageSize { get; set; } = 30;

    public string AppName { get; set; } = "Matrix Audio Studio";

    public string AppVersion { get; set; } = "1.0";

    public required string DbUrl { get; set; } = string.Empty;

    public required string DbName { get; set; } = string.Empty;

    public string VideoRootFolder { get; set; } = string.Empty;

    public bool UseGpu { get; set; } = true;

    public Dictionary<string, string> PathMapping { get; set; } = [];
}
