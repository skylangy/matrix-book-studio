using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using IO = System.IO;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class MaintenanceController(
    IOptions<AppConfiguration> bookConfig,
    IEntityRepository entityRepository,
    ILogger<MaintenanceController> logger) : ControllerBase
{
    private readonly ILogger<MaintenanceController> _logger = logger;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly AppConfiguration _bookConfig = bookConfig.Value;

    [HttpPost("arrange/folder", Name = "arrageFolder")]
    public async Task<OperationResult> ArrangeFolder()
    {
        var bookLocation = _bookConfig.BooksLocation;
        var books = await _entityRepository.GetAllAsync<Book>();

        var booksToMove = new List<string>();
        foreach (var book in books.Where(x => !string.IsNullOrWhiteSpace(x.Author)).OrderBy(x => x.Author))
        {
            try
            {
                var bookFolder = IO.Path.Combine(bookLocation, book.Title);
                if (!IO.Directory.Exists(bookFolder))
                {
                    continue;
                }
                var author = book.Author!.Trim();
                var authorFolder = IO.Path.Combine(bookLocation, author);
                authorFolder.EnsureDirectoryExists();

                var destFolder = IO.Path.Combine(authorFolder, book.Title.Trim());
                destFolder.EnsureDirectoryExists();

                IO.Directory.Move(bookFolder, destFolder);

                booksToMove.Add($"Book '{book.Title}' is moved to {destFolder}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error moving book with title '{Title}'", book.Title);
                continue;
            }
        }

        return new OperationResult
        {
            Success = true,
            Message = "Folder arranged",
            Payload = booksToMove
        };
    }

    [HttpPost("arrange/authors", Name = "arrageAuthors")]
    public async Task<OperationResult> ArrangeAuthors()
    {
        var books = await _entityRepository.GetAllAsync<Book>();
        var authorNames = books.Select(x => x.Author).Distinct().OrderBy(x => x).ToList();
        var authors = await _entityRepository.GetAllAsync<Author>();

        List<string> messages = [];
        foreach (var name in authorNames)
        {
            if (!authors.Any(x => x.Name == name))
            {
                var author = new Author
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name!.Trim(),
                    Alias = name!.Trim(),
                    Description = string.Empty,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                };
                var result = await _entityRepository.UpdateAsync(author);
                messages.Add($"Author '{name}' added with {result}");
            }
        }

        return new OperationResult
        {
            Success = true,
            Message = "Authors updated",
            Payload = messages
        };
    }

    [HttpPost("clean/combinevideos", Name = "cleanCombineVideoFolder")]
    public Task<OperationResult> CleanCombineVideoFolder()
    {
        var result = new OperationResult();
        var bookLocation = _bookConfig.BooksLocation;

        try
        {
            var combineVideosFolders = IO.Directory.GetDirectories(bookLocation, ResourceTypes.CombineVideos, IO.SearchOption.AllDirectories);

            if (combineVideosFolders.Any())
            {
                foreach (var folder in combineVideosFolders)
                {
                    // Remove each folder and its contents
                    IO.Directory.Delete(folder, true);
                    _logger.LogInformation("Folder '{Folder}' has been removed.", folder);
                }

                result.Success = true;
                result.Message = $"{combineVideosFolders.Length} folder(s) named 'combine videos' have been removed.";
            }
            else
            {
                _logger.LogInformation("No folder named 'combine videos' found.");
                result.Success = false;
                result.Message = "No folder named 'combine videos' found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while removing 'combine videos' folder.");
            result.Success = false;
            result.Message = "An error occurred while removing the folder.";
        }
        return Task.FromResult(result);
    }

    [HttpPost("clean/wav", Name = "cleanWav")]
    public Task<OperationResult> CleanOldFolders()
    {
        var result = new OperationResult();
        var bookLocation = _bookConfig.BooksLocation;

        try
        {
            var oldFolders = IO.Directory.GetDirectories(bookLocation, ResourceTypes.Wav, IO.SearchOption.AllDirectories)
                                          .Where(folder => IO.Directory.GetLastWriteTime(folder) < DateTime.Now.AddDays(-20))
                                          .ToList();

            if (oldFolders.Any())
            {
                foreach (var folder in oldFolders)
                {
                    // Remove each folder and its contents
                    IO.Directory.Delete(folder, true);
                    _logger.LogInformation("Folder '{Folder}' has been removed.", folder);
                }

                result.Success = true;
                result.Message = $"{oldFolders.Count} folder(s) older than 20 days have been removed.";
            }
            else
            {
                _logger.LogInformation("No folders older than 20 days found.");
                result.Success = false;
                result.Message = "No folders older than 20 days found.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while removing old folders.");
            result.Success = false;
            result.Message = "An error occurred while removing old folders.";
        }
        return Task.FromResult(result);
    }
}
