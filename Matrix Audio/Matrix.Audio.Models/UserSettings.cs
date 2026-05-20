namespace Matrix.Audio.Models;
public class UserSettings : Entity
{
    public required string UserId { get; set; }

    public List<UserSetting> Settings { get; set; } = [];

}
