using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace AudioBookStudio.Common.Services;
public class ImageService(ILogger<ImageService> logger) : IImageService
{
    private readonly ILogger<ImageService> _logger = logger;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
    public (int, int) GetImageSize(string imagePath)
    {
        try
        {
            using var image = Image.FromFile(imagePath);

            return (image.Width, image.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image info");

        }
        return (0, 0);
    }
}
