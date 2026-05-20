using DatabaseTools.Models;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace DatabaseTools.Services;
public class ImageService(ILogger<ImageService> logger) : IImageService
{
    private readonly ILogger<ImageService> _logger = logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public ImageInfo GetImageInfo(string path)
    {
        var info = new ImageInfo();
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return info;
        }

        try
        {
            using (var image = Image.FromFile(path))
            {
                info.Width = image.Width;
                info.Height = image.Height;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info");

            return info;
        }
        return info;
    }
}
