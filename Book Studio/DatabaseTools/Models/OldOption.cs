namespace DatabaseTools.Models;
public class OldOption : OldEntity
{
    public required string Name { get; set; }
    public string? DisplayName { get; set; }
    public string? Group { get; set; }
    public string? ValueType { get; set; }
    public string? Value { get; set; }
}
