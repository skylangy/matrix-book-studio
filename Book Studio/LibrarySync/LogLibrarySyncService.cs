using Microsoft.Extensions.Logging;

namespace LibrarySync;
public class LogLibrarySyncService(ILogger<LogLibrarySyncService> logger) : ILibrarySyncService
{
    private readonly ILogger<LogLibrarySyncService> _logger = logger;

    public Task SyncLibrary(string source, string destination)
    {
        _logger.LogInformation("Syncing library from {Source} to {Destination}", source, destination);

        foreach (var authorDir in Directory.GetDirectories(source))
        {
            var authorName = Path.GetFileName(authorDir);
            _logger.LogInformation("Author: {AuthorName}", authorName);

            string sourceAuthorImages = Path.Combine(authorDir, "images");
            if (Directory.Exists(sourceAuthorImages))
            {
                _logger.LogInformation("\tCopying author images from: {SourceAuthorImages}", sourceAuthorImages);
            }

            foreach (var bookDir in Directory.GetDirectories(authorDir))
            {
                var bookName = Path.GetFileName(bookDir);
                _logger.LogInformation("\tBook: {BookName}", bookName);

                string sourceBookImages = Path.Combine(bookDir, "images");
                if (Directory.Exists(sourceBookImages))
                {
                    _logger.LogInformation("\t\tCopying book images from: {SourceBookImages}", sourceBookImages);
                }

                string sourceMp3 = Path.Combine(bookDir, "mp3");
                if (Directory.Exists(sourceMp3))
                {
                    _logger.LogInformation("\t\tCopying mp3 from: {SourceMp3}", sourceMp3);
                }

                string sourceTxt = Path.Combine(bookDir, "txt");
                if (Directory.Exists(sourceTxt))
                {
                    _logger.LogInformation("\t\tCopying txt from: {SourceTxt}", sourceTxt);
                }
            }
        }

        return Task.CompletedTask;
    }
}
