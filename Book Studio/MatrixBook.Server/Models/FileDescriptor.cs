namespace MatrixBook.Server.Models;

public class FileDescriptor
{
    public string? Name { get; set; }
    public string? Path { get; set; }
    public long Size { get; set; }
    public long Duration { get; set; }
}
