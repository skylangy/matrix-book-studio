namespace Matrix.Audio.Models;
public class AlbumCollection : Entity
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public Dictionary<string, int> AlbumIds { get; set; } = [];
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime DateUpdated { get; set; } = DateTime.Now;

    public List<string> GetSortedAlbumIds()
    {
        return AlbumIds.OrderBy(x => x.Value).Select(x => x.Key).ToList();
    }
}
