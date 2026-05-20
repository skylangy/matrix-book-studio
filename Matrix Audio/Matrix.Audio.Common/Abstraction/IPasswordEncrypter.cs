namespace Matrix.Audio.Common.Abstraction;
public interface IPasswordEncrypter
{
    string Encrypt(string password);
    bool Verify(string hashedPassword, string password);
}
