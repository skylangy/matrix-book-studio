namespace Matrix.Audio.Server.ViewModels;

/// <summary>
/// Represents Category or Tag.
/// </summary>
public class GroupViewModel
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
}
