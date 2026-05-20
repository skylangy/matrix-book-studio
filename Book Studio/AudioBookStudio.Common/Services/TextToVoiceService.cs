using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;

public class TextToVoiceService(
    IOptions<AppConfiguration> configuration,
    Func<string, ISpeechSynthesizer> factory,
    ISynthesizePreProcessor synthesizePreProcessor,
    ILogger<TextToVoiceService> logger) : ITextToVoiceService
{
    private readonly AppConfiguration _configuration = configuration.Value;
    private readonly ILogger<TextToVoiceService> _logger = logger;
    private readonly ISynthesizePreProcessor _synthesizePreProcessor = synthesizePreProcessor;
    private readonly Func<string, ISpeechSynthesizer> _synthesizerFactory = factory;

    public async Task<TextToVoiceResult> Convert(string content, TextToVoiceServiceContext context)
    {
        var convertResult = new TextToVoiceResult();

        var synthesizeContext = new SynthesizeContext
        {
            Language = context.Language,
            VoiceName = context.VoiceName,
            UseSsml = context.UseSsml,
            OutputPath = context.OutputPath,
            FileName = context.FileName
        };

        var synthesizer = _synthesizerFactory(context.SpeechService);
        content = await _synthesizePreProcessor.ProcessAsync(content);
        var result = await synthesizer.SynthesizeAsync(content, synthesizeContext);

        if (result.Success)
        {
            convertResult.Success = true;
            convertResult.OutputFile = result.OutputFile;
        }
        else
        {
            convertResult.Success = false;
            convertResult.Error = result.Error;
        }

        return convertResult;
    }
}