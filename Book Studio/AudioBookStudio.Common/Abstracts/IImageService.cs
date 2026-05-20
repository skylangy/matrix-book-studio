namespace AudioBookStudio.Common.Abstracts;
public interface IImageService
{
    (int Width, int Height) GetImageSize(string imagePath);
}
