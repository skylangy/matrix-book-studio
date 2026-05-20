namespace AudioBookStudio.Films.Common;
public static class FileExtensions
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png"
    };

    public static IEnumerable<string> EnumerateImageFiles(this DirectoryInfo dirInfo)
    {

        return dirInfo.EnumerateFiles()
                      .Where(file => AllowedExtensions.Contains(Path.GetExtension(file.FullName)))
                      .SortNatural();
    }

    public static IEnumerable<string> EnumerateImageFiles(this string folder)
    {
        DirectoryInfo dirInfo = new(folder);
        return dirInfo.EnumerateImageFiles();
    }

    public static IEnumerable<string> SortNatural(this IEnumerable<FileInfo> files)
    {
        return files.OrderBy(file => Path.GetFileName(file.Name), new NaturalStringComparer()).Select(x => x.FullName);
    }

    public static IEnumerable<string> SortNatural(this IEnumerable<string> files)
    {
        return files.OrderBy(path => Path.GetFileName(path), new NaturalStringComparer());
    }
}
