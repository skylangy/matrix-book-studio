using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;

public interface IVideoGenerator
{
    Task<VideoGeneratorResult> Generate(VideoGeneratorContext context);
}