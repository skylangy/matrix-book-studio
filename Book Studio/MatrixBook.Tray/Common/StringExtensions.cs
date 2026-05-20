namespace MatrixBook.Tray.Common;
public static class StringExtensions
{
    public static string ConvertToWindowsPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        var windowsPath = path.Replace('/', '\\');

        // If it's a UNC path or already has a drive letter (like "C:\")
        if (windowsPath.StartsWith("\\\\") ||
            (windowsPath.Length >= 2 && char.IsLetter(windowsPath[0]) && windowsPath[1] == ':'))
        {
            return windowsPath;
        }

        // Otherwise, treat as relative — don't prepend a slash
        return windowsPath;
    }

}
