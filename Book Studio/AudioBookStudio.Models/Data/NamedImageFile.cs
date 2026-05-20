namespace AudioBookStudio.Models.Data;
public class NamedImageFile
{
    public string Name { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public NamedImageFile() { }
    public NamedImageFile(string name, string filePath)
    {
        Name = name;
        FilePath = filePath;
    }
}
