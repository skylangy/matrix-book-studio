namespace AudioBookStudio.Films.Models;
public class FontFile(string name, string path)
{
    public string Name { get; set; } = name;
    public string Path { get; set; } = path;

    public string EscapePath()
    {
        if (string.IsNullOrEmpty(Path)) return "";
        return Path.Replace("\\", "/").Replace(":", "\\:").Replace("\"", "\\\"");
    }

    public override string ToString() => $"FontFile(name=\"{Name}\", path=\"{Path}\")";

    public bool Exists() => File.Exists(Path);
}