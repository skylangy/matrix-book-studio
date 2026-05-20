namespace Matrix.Audio.Models;
public class LevelFeatures : Entity
{
    public required string Level { get; set; }
    public required List<string> Features { get; set; } = [];
}
