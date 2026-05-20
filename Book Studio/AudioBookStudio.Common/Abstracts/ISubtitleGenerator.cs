using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface ISubtitleGenerator
{
    Task GenerateSubtitlesAsync(SubtitleGenerateContext context);

    Task GenerateSubtitlesFromFolderAsync(SubtitleGenerateContext context);
}
