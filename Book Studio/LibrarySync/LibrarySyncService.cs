using Microsoft.Extensions.Logging;

namespace LibrarySync;
public class LibrarySyncService(ILogger<LibrarySyncService> logger) : ILibrarySyncService
{
    private readonly ILogger<LibrarySyncService> _logger = logger;
    private readonly bool _dryRun = false;

    public async Task SyncLibrary(string source, string destination)
    {
        _logger.LogInformation("Syncing library from '{Source}' to '{Destination}'", source, destination);

        foreach (var authorDir in Directory.GetDirectories(source))
        {
            string authorName = Path.GetFileName(authorDir);
            string authorDestDir = Path.Combine(destination, authorName);

            _logger.LogInformation("Syncing author from '{}' to '{}'", authorDir, authorDestDir);

            // Sync author images folder
            string sourceAuthorImages = Path.Combine(authorDir, "images");
            string destAuthorImages = Path.Combine(authorDestDir, "images");
            if (Directory.Exists(sourceAuthorImages))
            {
                _logger.LogInformation("\t Copying author images from '{Source}' to '{Destination}'", sourceAuthorImages, destAuthorImages);
                await CopyDirectory(sourceAuthorImages, destAuthorImages);
            }

            // Sync books under the author
            foreach (var bookDir in Directory.GetDirectories(authorDir))
            {
                string bookName = Path.GetFileName(bookDir);
                string bookDestDir = Path.Combine(authorDestDir, bookName);

                _logger.LogInformation("\t Syncing book from '{Source}' to '{Destination}'", bookDir, bookDestDir);

                // Sync folders (images, mp3, txt)
                await SyncFolder(bookDir, bookDestDir, "images");
                await SyncFolder(bookDir, bookDestDir, "mp3");
                await SyncFolder(bookDir, bookDestDir, "txt");
            }
        }

        _logger.LogInformation("Library sync completed successfully.");
    }

    private async Task SyncFolder(string bookDir, string bookDestDir, string folderName)
    {
        string sourceFolder = Path.Combine(bookDir, folderName);
        string destFolder = Path.Combine(bookDestDir, folderName);

        if (Directory.Exists(sourceFolder))
        {
            //_logger.LogInformation("\t\t Syncing '{Folder}' from '{Source}' to '{Destination}'", folderName, sourceFolder, destFolder);
            await CopyDirectory(sourceFolder, destFolder);
        }
    }

    private async Task CopyDirectory(string sourceDir, string destinationDir)
    {
        _logger.LogInformation($"\t\t Copying '{sourceDir}' to '{destinationDir}'");

        if (!Directory.Exists(destinationDir))
        {
            Directory.CreateDirectory(destinationDir);
        }

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            string destFile = Path.Combine(destinationDir, Path.GetFileName(file));

            if (_dryRun)
            {
                _logger.LogInformation("\t\t\t Copying '{File}' to '{Destination}'", file, destFile);
            }
            else
            {
                if (!File.Exists(destFile))
                {
                    File.Copy(file, destFile, true);
                }
            }
        }

        foreach (var subDir in Directory.GetDirectories(sourceDir))
        {
            string destSubDir = Path.Combine(destinationDir, Path.GetFileName(subDir));
            await CopyDirectory(subDir, destSubDir);
        }
    }
}
