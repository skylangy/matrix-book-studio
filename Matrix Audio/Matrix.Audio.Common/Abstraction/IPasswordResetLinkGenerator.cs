namespace Matrix.Audio.Common.Abstraction;
public interface IPasswordResetLinkGenerator
{
    string GenerateResetLink(string userEmail);
}
