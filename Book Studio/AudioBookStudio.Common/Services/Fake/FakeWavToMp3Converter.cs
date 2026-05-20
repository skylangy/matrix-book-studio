using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;

namespace AudioBookStudio.Common.Services.Fake;

public sealed class FakeWavToMp3Converter(ILogger<FakeWavToMp3Converter> logger) : IWavToMp3Converter
{
    public async Task<ConvertResult> Convert(ConvertContext context)
    {
        var result = new ConvertResult();
        if (context.Sources.Any())
        {
            logger.LogInformation("Start converting WAV to MP3 to {}", context.Destination);

            var temp = Path.GetTempFileName();
            try
            {
                using var outStream = new FileStream(temp, FileMode.Create, FileAccess.Write);
                using var writer = new StreamWriter(outStream);

                foreach (var source in context.Sources)
                {
                    logger.LogInformation("Reading source: {}", source);
                    using var reader = new FileStream(source, FileMode.Open, FileAccess.Read);
                    await reader.CopyToAsync(outStream);
                }
                outStream.Flush();

                logger.LogInformation("Writing to destination: {}", context.Destination);
                File.Copy(temp, context.Destination, true);

                result.Success = true;
            }
            catch (Exception e)
            {
                context.ExceptionHandler?.Invoke(e);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                logger.LogInformation("Finish converting WAV to MP3 to {}", context.Destination);
            }
        }

        return result;
    }

    public Task<ConvertResult> Enhance(ConvertContext context)
    {
        var result = new ConvertResult();
        return Task.FromResult(result);
    }
}