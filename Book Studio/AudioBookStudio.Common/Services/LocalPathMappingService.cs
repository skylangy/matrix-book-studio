using AudioBookStudio.Common.Abstracts;

namespace AudioBookStudio.Common.Services;
public class LocalPathMappingService : IPathMappingService
{
    public string Map(string path)
    {
        return path;
    }
}
