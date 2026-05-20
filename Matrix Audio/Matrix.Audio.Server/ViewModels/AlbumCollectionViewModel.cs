namespace Matrix.Audio.Server.ViewModels;

public class AlbumCollectionViewModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int Count { get; set; }
    public List<AlbumViewModel> Albums { get; set; } = [];
    public DateTime DateCreated { get; set; } = DateTime.Now;
    public DateTime DateUpdated { get; set; } = DateTime.Now;
}
