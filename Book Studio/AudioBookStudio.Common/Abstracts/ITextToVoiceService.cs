using AudioBookStudio.Common.Models;
using AudioBookStudio.Common.Services;

namespace AudioBookStudio.Common.Abstracts;

public interface ITextToVoiceService
{
    Task<TextToVoiceResult> Convert(string content, TextToVoiceServiceContext context);
}