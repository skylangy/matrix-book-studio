using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace AudioBookStudio.Common.Services;
public class EdgeTtsClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private const string DefaultVoice = "en-US-AriaNeural";
    private const string OutputFormat = "riff-24khz-16bit-mono-pcm";
    private bool _needCleanTempFiles = false;

    public EdgeTtsClient(string baseUrl, ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        _httpClient.DefaultRequestHeaders.Add("X-Microsoft-OutputFormat", OutputFormat);
    }

    public async Task<string> GetStatusAsync()
    {
        var response = await _httpClient.GetAsync("/");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    public async Task<Voice[]> ListVoicesAsync()
    {
        var response = await _httpClient.GetAsync("/cognitiveservices/voices/list");
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<Voice[]>(json) ?? Array.Empty<Voice>();
    }

    public async Task<byte[]> GenerateSpeechSsmlAsync(string ssml)
    {
        try
        {
            var content = new StringContent(ssml, Encoding.UTF8, "application/ssml+xml");
            var response = await _httpClient.PostAsync("/tts/ssml", content);
            response.EnsureSuccessStatusCode();
            byte[] audio = await response.Content.ReadAsByteArrayAsync();
            return audio;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating speech from SSML: {Message}", ex.Message);
            throw;
        }
        finally
        {
            await CleanTempFiles();
        }
    }

    public async Task<byte[]> GenerateSpeechTextAsync(string text, string language = null, string voice = null)
    {
        try
        {
            var payload = new { text, voice = voice ?? DefaultVoice };
            string json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/tts/text", content);
            response.EnsureSuccessStatusCode();
            byte[] audio = await response.Content.ReadAsByteArrayAsync();
            return audio;
        }
        finally
        {
            await CleanTempFiles();
        }
    }

    private async Task CleanTempFiles()
    {
        if (!_needCleanTempFiles)
            return;
        var response = await _httpClient.GetAsync("/clean");
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}

public class Voice
{
    public string Name { get; set; }
    public string ShortName { get; set; }
    public string Gender { get; set; }
    public string Locale { get; set; }
    public string SampleRateHertz { get; set; }
    public string VoiceType { get; set; }

    public override string ToString() => $"{Name} ({Gender}, {Locale})";
}
