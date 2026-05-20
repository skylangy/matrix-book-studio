using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;

public interface IWavToMp3Converter
{
    Task<ConvertResult> Convert(ConvertContext context);

    Task<ConvertResult> Enhance(ConvertContext context);
}