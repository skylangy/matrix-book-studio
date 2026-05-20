namespace Matrix.Audio.Common.Extensions;
public static class IoExtensions
{
    public static void EnsureDirectoryExists(this string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
