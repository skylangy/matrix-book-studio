using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Films;
using AudioBookStudio.Films.Extensions;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class VideoExportService(
    IOptions<AppConfiguration> configuration,
    ITextToVoiceService textToVoiceService,
    IWavToMp3Converter wavToMp3Converter,
    ILogger<VideoExportService> logger) : IVideoExportService
{
    private readonly ITextToVoiceService _textToVoiceService = textToVoiceService;
    private readonly IWavToMp3Converter _wavToMp3Converter = wavToMp3Converter;
    private readonly ILogger<VideoExportService> _logger = logger;
    private readonly AppConfiguration _configuration = configuration.Value;

    public async Task Export(VideoMeta videoMeta)
    {
        await BuildVideo(videoMeta);
    }

    public async Task ExportVideoOnly(VideoMeta videoMeta)
    {
        await BuildVideo(videoMeta, false);
    }

    private async Task<bool> BuildVideo(VideoMeta videoMeta, bool buildAudio = true)
    {
        var resourceRoot = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.ResourceRoot);

        var fileName = videoMeta.GetExportName();
        _logger.LogInformation("Exporting video for {Title} with file name '{FileName}'", videoMeta.Title, fileName);

        var wavFolder = Path.Combine(resourceRoot, ResourceTypes.Wav);
        wavFolder.EnsureDirectoryExists();

        var mp3Folder = Path.Combine(resourceRoot, ResourceTypes.Mp3);
        mp3Folder.EnsureDirectoryExists();
        var mp3File = Path.Combine(mp3Folder, $"{fileName}.{ResourceTypes.Mp3}");

        if (buildAudio)
        {
            _logger.LogInformation("Start converting to wav");

            await _textToVoiceService.Convert(videoMeta.Content.RemoveLineBreaks(), new()
            {
                SpeechService = videoMeta.SpeechService,
                Language = videoMeta.Language,
                VoiceName = videoMeta.VoiceName,
                OutputPath = wavFolder,
                FileName = $"{fileName}.{ResourceTypes.Wav}",
            });

            var wavFile = Path.Combine(resourceRoot, ResourceTypes.Wav, $"{fileName}.{ResourceTypes.Wav}");
            var wavExists = await wavFile.WaitForFileCreated();
            _logger.LogInformation("Wav conversion completed {wavExists}", wavExists);
            if (!wavExists)
            {
                _logger.LogError("Wav file {WavFile} does not exist after conversion", wavFile);
                return false;
            }

            _logger.LogInformation("Converting wav to mp3 from {}", wavFile);
            await _wavToMp3Converter.Convert(new()
            {
                Book = videoMeta.Name,
                Chapter = videoMeta.Title,
                Sources = [wavFile],
                Destination = mp3File,
            });
        }

        var mp3Exists = await mp3File.WaitForFileCreated();
        _logger.LogInformation("MP3 conversion completed {mp3Exists}", mp3Exists);
        if (!mp3Exists)
        {
            _logger.LogError("MP3 file {Mp3File} does not exist after conversion", mp3File);
            return false;
        }

        var outputFolder = Path.Combine(_configuration.VideoRootFolder, ResourceTypes.Output);
        outputFolder.EnsureDirectoryExists();

        var videoOutput = Path.Combine(outputFolder, $"{videoMeta.Category}");
        videoOutput.EnsureDirectoryExists();
        var videoFileName = videoMeta.GetOutputFileName(_configuration.VideoRootFolder);
        var bookSnippet = videoMeta.ToFilmSnippet(resourceRoot, videoFileName);
        var logo = videoMeta.Logo.ToFilmLogo(resourceRoot);

        await FilmBuilder.BuildBibleShortVideo(bookSnippet, logo, videoFileName, _logger);

        var videoExists = await videoFileName.WaitForFileCreated();
        _logger.LogInformation("Video export completed for {Title} with file name '{FileName}' {exists}", videoMeta.Title, videoFileName, videoExists);
        return true;
    }

}
