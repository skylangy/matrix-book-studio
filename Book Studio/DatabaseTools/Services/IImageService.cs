using DatabaseTools.Models;

namespace DatabaseTools.Services;
public interface IImageService
{
    ImageInfo GetImageInfo(string path);
}
