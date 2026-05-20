namespace Matrix.Audio.Models;
public class EmailSubscribe : Entity
{
    public required string Email { get; set; }
    public DateTime DateSubscribed { get; set; }
    public bool IsActive { get; set; }
}
