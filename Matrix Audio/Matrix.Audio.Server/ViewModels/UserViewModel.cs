namespace Matrix.Audio.Server.ViewModels;

public class UserViewModel : ViewModel
{
    public string? ImageId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int ReadBooks { get; set; }
    public int Level { get; set; }

    public DateTime DateCreated { get; set; }
    public DateTime DateUpdated { get; set; }
    public DateTime DateLastLogin { get; set; }
}
