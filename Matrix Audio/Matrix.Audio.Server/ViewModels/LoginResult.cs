namespace Matrix.Audio.Server.ViewModels;

public class LoginResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? Message { get; set; }

    public UserViewModel? User { get; set; }

    public static LoginResult Fail => new() { Success = false };

    public static LoginResult Error(string message) => new() { Success = false, Message = message };
}
