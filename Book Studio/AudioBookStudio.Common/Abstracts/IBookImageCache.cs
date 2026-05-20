namespace AudioBookStudio.Common.Abstracts;
public interface IBookImageCache
{
    (bool, string) GetImagePath(string bookId);

    void SetImagePath(string bookId, string imagePath);
}
