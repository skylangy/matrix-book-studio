namespace AudioBookStudio.Films.Common;
public static class StringExtensions
{
    public static string QuotePath(this string? path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;
        return $"\"{path}\"";
    }

    public static string ToSafeFileName(this string? name)
    {
        if (string.IsNullOrEmpty(name))
            return string.Empty;


        var invalidChars = Path.GetInvalidFileNameChars();
        foreach (var c in invalidChars)
        {
            name = name.Replace(c, '_');
        }

        if (name.Length > 255)
            name = name[..255];
        return name;
    }

    public static char[] GetUnsupportChars()
    {
        return [':', '：'];
    }
}
