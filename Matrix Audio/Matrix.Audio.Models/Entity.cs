namespace Matrix.Audio.Models;

public class Entity
{
    public required string Id { get; set; } = Guid.NewGuid().ToString();
}
