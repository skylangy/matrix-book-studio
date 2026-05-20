using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class EdgeSynthesizer : ISpeechSynthesizer
{
    private readonly EdgeSynthesizerConfig _configuration;
    private readonly ILogger<EdgeSynthesizer> _logger;
    private readonly Lazy<EdgeTtsClient> _edgeTtsClientLazy;
    private readonly Dictionary<string, string> _voiceMapping = new()
    {
        { "zh-CN-XiaochenNeural", "zh-CN-XiaoxiaoNeural" },
        { "zh-CN-YunzeNeural", "zh-CN-YunjianNeural" }
    };

    public EdgeSynthesizer(
        IOptions<EdgeSynthesizerConfig> configuration,
        ILogger<EdgeSynthesizer> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _edgeTtsClientLazy = new Lazy<EdgeTtsClient>(() =>
        {
            var client = new EdgeTtsClient(_configuration.Endpoint, _logger);
            return client;
        });
    }

    public EdgeTtsClient EdgeTtsClient => _edgeTtsClientLazy.Value;

    public async Task<SynthesizeResult> SynthesizeAsync(string content, SynthesizeContext context)
    {
        var result = new SynthesizeResult();
        byte[] audioData;
        if (context.UseSsml)
        {
            audioData = await ConvertSsmlAsync(content, context);
        }
        else
        {
            audioData = await ConvertRawTextAsync(content, context);
        }

        if (audioData != null && audioData.Length > 0)
        {
            var filePath = await SaveAudioData(audioData, context.OutputPath, context.FileName);
            result.Success = true;
            result.OutputFile = filePath;
        }
        else
        {
            result.Success = false;
            result.Error = "Failed to generate audio data.";
        }
        return result;
    }

    public string VoiceMapping(string voiceName, string language)
    {
        if (_voiceMapping.TryGetValue(voiceName, out var mappedVoice))
        {
            return mappedVoice;
        }
        return _configuration.VoiceName;
    }

    private async Task<byte[]> ConvertSsmlAsync(string content, SynthesizeContext context)
    {
        try
        {
            var voiceName = context.VoiceName; // VoiceMapping(context.VoiceName, context.Language);
            var ssml = content.ToSsml(voiceName, context.Language ?? _configuration.Language);
            var audioData = await EdgeTtsClient.GenerateSpeechSsmlAsync(ssml);

            return audioData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting SSML to audio: {}", ex.Message);
            return null;
        }
    }

    private async Task<byte[]> ConvertRawTextAsync(string content, SynthesizeContext context)
    {
        try
        {
            var voiceName = VoiceMapping(context.VoiceName, context.Language);
            var audioData = await EdgeTtsClient.GenerateSpeechTextAsync(content, context.Language ?? _configuration.Language, voiceName);
            return audioData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting raw text to audio");
            return null;
        }
    }

    private Task<string> SaveAudioData(byte[] audioData, string outputPath, string fileName)
    {
        try
        {
            var filePath = Path.Combine(outputPath, fileName);
            var tempFile = Path.GetTempFileName();

            File.WriteAllBytes(tempFile, audioData);
            File.Copy(tempFile, filePath, true);
            File.Delete(tempFile);

            return Task.FromResult(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving audio data");
            return Task.FromResult<string>(null);
        }
    }
}
