using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class LinuxPathMappingService(IOptions<AppConfiguration> configuration) : IPathMappingService
{
    private readonly AppConfiguration _configuration = configuration.Value;

    public string Map(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        var result = path;
        foreach (var item in _configuration.PathMapping)
        {
            if (result.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
            {
                result = path.Replace(item.Key, item.Value, StringComparison.OrdinalIgnoreCase);
            }
        }
        return result;
    }
}
