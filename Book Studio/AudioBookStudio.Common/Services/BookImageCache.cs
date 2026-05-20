using AudioBookStudio.Common.Abstracts;
using System.Collections.Concurrent;

namespace AudioBookStudio.Common.Services;
public class BookImageCache : IBookImageCache
{
    private readonly ConcurrentDictionary<string, string> _imagePaths = [];

    public (bool, string) GetImagePath(string bookId)
    {
        _imagePaths.TryGetValue(bookId, out var imagePath);

        return (!string.IsNullOrWhiteSpace(imagePath), imagePath);
    }

    public void SetImagePath(string bookId, string imagePath)
    {
        _imagePaths[bookId] = imagePath;
    }
}
