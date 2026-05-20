namespace Matrix.Audio.Server.ViewModels;

public class LoginViewModel
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public string? Token { get; set; }
}
