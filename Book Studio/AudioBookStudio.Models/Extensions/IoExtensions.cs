namespace AudioBookStudio.Models.Extensions;

public static class IoExtensions
{
    public static void EnsureDirectoryExists(this string path)
    {
        if (File.Exists(path))
        {
            throw new IOException($"A file exists at the directory path: {path}");
        }

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static async Task SafeWrite(this string path, string content, Action<Exception> handler)
    {
        var temp = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(temp, content);
            File.Copy(temp, path, true);
        }
        catch (Exception ex)
        {
            handler?.Invoke(ex);
        }
        finally
        {
            File.Delete(temp);
        }
    }


}