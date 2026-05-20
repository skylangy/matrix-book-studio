
using AudioBookStudio.Models.Data;

namespace MatrixBook.Server.Models;

public class Option : Entity
{
    public required string Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Group { get; set; }
    public string? ValueType { get; set; }
    public string? Value { get; set; }
}
