namespace Matrix.Audio.Server.ViewModels;

public class ArtistViewModel : ViewModel
{
    public string? Name { get; set; }
    public string? Alias { get; set; }
    public string? Description { get; set; }
    public string? Image { get; set; }
    public int AlbumCount { get; set; } = 0;
    public IEnumerable<AlbumViewModel> Albums { get; set; } = [];

    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
}
