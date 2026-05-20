using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class AzureSynthesizer : ISpeechSynthesizer
{
    private readonly AzureSynthesizerConfig _configuration;
    private readonly ILogger<AzureSynthesizer> _logger;
    private readonly SpeechConfig _speechConfig;
    private readonly ServiceSubscription _subscription;
    private int _counter = 0;
    private DateTime? _lastRequest;

    public AzureSynthesizer(
        IOptions<AzureSynthesizerConfig> configuration,
        ILogger<AzureSynthesizer> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;

        _subscription = _configuration.Subscriptions.FirstOrDefault(x => x.IsEnabled);
        _speechConfig = SpeechConfig.FromSubscription(_subscription.Key, _subscription.Region);
    }

    public async Task<SynthesizeResult> SynthesizeAsync(string content, SynthesizeContext context)
    {
        var result = new SynthesizeResult();

        try
        {
            _speechConfig.SpeechRecognitionLanguage = context.Language;
            _speechConfig.SpeechSynthesisLanguage = context.Language;
            _speechConfig.SpeechSynthesisVoiceName = context.VoiceName;
            _speechConfig.EnableDictation();

            _logger.LogInformation("Converting text to voice using subscription: {}", _subscription.Name);
            if (context.UseSsml)
            {
                result = await ConvertSsmlAsync(content, context).Retry();
            }
            else
            {
                result = await ConvertRawTextAsync(content, context).Retry();
            }

            if (_subscription.RequestDelayMs > 0)
            {
                _logger.LogInformation("Waiting for {} ms", _subscription.RequestDelayMs);
                await Task.Delay(_subscription.RequestDelayMs);
            }

            if (!_lastRequest.HasValue)
                _lastRequest = DateTime.Now;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Convert text to voice with error: {}", ex.Message);
            result.Success = false;
            result.Error = ex.Message;
        }
        finally
        {
            _counter++;

            if (DateTime.Now.Subtract(_lastRequest.Value).TotalSeconds > 50 && _counter >= 15)
            {
                _logger.LogInformation("Waiting for 30 seconds to avoid service throttling");
                await Task.Delay(1000 * 30);
                _counter = 0;
                _lastRequest = DateTime.Now;
            }
        }

        return result;
    }

    public string VoiceMapping(string voiceName, string language)
    {
        return voiceName;
    }

    private async Task<SynthesizeResult> ConvertRawTextAsync(string content, SynthesizeContext context)
    {
        var convertResult = new SynthesizeResult();
        try
        {
            using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
            using var result = await synthesizer.SpeakTextAsync(content);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                await SaveAudio(context, convertResult, result);
            }
            else
            {
                convertResult.Success = false;
                convertResult.Error = $"{result.Reason} by the service";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            convertResult.Success = false;
            convertResult.Error = ex.Message;
        }
        return convertResult;
    }

    private async Task<SynthesizeResult> ConvertSsmlAsync(string content, SynthesizeContext context)
    {
        var convertResult = new SynthesizeResult();
        try
        {
            string ssml = content.ToSsml(_speechConfig.SpeechSynthesisVoiceName, _speechConfig.SpeechSynthesisLanguage);

            _logger.LogInformation("SSML: {}", ssml);

            using var synthesizer = new SpeechSynthesizer(_speechConfig, null);
            using var result = await synthesizer.SpeakSsmlAsync(ssml);
            if (result.Reason == ResultReason.SynthesizingAudioCompleted)
            {
                await SaveAudio(context, convertResult, result);
            }
            else
            {
                var detailed = SpeechSynthesisCancellationDetails.FromResult(result);
                convertResult.Success = false;
                convertResult.Error = $"{result.Reason} by the service, {detailed.ErrorDetails}";

                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            convertResult.Success = false;
            convertResult.Error = ex.Message;
        }
        return convertResult;
    }

    private static async Task SaveAudio(SynthesizeContext context, SynthesizeResult convertResult, SpeechSynthesisResult result)
    {
        var fullPath = Path.Combine(context.OutputPath, context.FileName);
        context.OutputPath.EnsureDirectoryExists();

        var tempFile = Path.GetTempFileName();
        var audioStream = AudioDataStream.FromResult(result);
        await audioStream.SaveToWaveFileAsync(tempFile);

        File.Copy(tempFile, fullPath, true);
        File.Delete(tempFile);

        convertResult.Success = true;
        convertResult.OutputFile = fullPath;
    }
}
