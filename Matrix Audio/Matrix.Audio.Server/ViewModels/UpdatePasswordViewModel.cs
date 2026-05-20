namespace Matrix.Audio.Server.ViewModels;

public class UpdatePasswordViewModel
{
    public required string Email { get; set; }
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}
