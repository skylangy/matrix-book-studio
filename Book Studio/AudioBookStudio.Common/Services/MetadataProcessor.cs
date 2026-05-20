using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Extensions;
using AudioBookStudio.Common.IO;
using AudioBookStudio.Models.Common;
using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class MetadataProcessor(
    IOptions<AppConfiguration> appConfiguration,
    IEntityRepository entityRepository,
    ILogger<MetadataProcessor> logger) : IMetadataProcessor
{
    private readonly AppConfiguration _appConfiguration = appConfiguration.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<MetadataProcessor> _logger = logger;

    public async Task GenerateAuthorMeta(Author author)
    {
        _logger.LogInformation("Generating metadata for author {Author}", author.Name);
        var path = Path.Combine(_appConfiguration.BooksLocation, author.Name, MetadataConsts.AuthorMetaFile);

        if (File.Exists(path))
        {
            var metadata = await path.LoadAuthorMetaFromFile();
            if (metadata != null && metadata.DateUpdated == author.DateUpdated)
            {
                _logger.LogInformation("Metadata for author {Author} is up to date", author.Name);
                return;
            }
        }

        var meta = author.ToMeta();
        meta.Image = $"{author.Name}{ResourceTypes.SplashType}";

        await meta.WriteMetaToFile(path);
    }

    public async Task GenerateBookMeta(Book book)
    {
        _logger.LogInformation("Generating metadata for book {Book}", book.Title);

        var path = Path.Combine(_appConfiguration.BooksLocation, book.Author, book.Title, MetadataConsts.AlbumMetaFile);
        if (File.Exists(path))
        {
            var metadata = await path.LoadBookMetaFromFile();
            if (metadata != null && metadata.DateUpdated == book.DateUpdated)
            {
                _logger.LogInformation("Metadata for book {Book} is up to date", book.Title);
                return;
            }
        }

        try
        {
            var meta = book.ToMeta();
            if (book.TagIds.Count != 0)
            {
                var tags = await _entityRepository.QueryTags(book);
                meta.Tags = [.. tags.Select(t => t.Name)];
            }

            if (book.CategoryIds.Count != 0)
            {
                var categories = await _entityRepository.QueryCategories(book);
                meta.Categories = [.. categories.Select(c => c.Name)];
            }

            meta.ImageWideSplash = $"{book.Title}-wide-bg{ResourceTypes.SplashType}";
            meta.ImageSquareSplash = $"{book.Title}-square-bg{ResourceTypes.SplashType}";

            var mp3Folder = Path.Combine(_appConfiguration.BooksLocation, book.Author, book.Title, "mp3");
            var txtFolder = Path.Combine(_appConfiguration.BooksLocation, book.Author, book.Title, "txt");

            mp3Folder.EnsureDirectoryExists();
            txtFolder.EnsureDirectoryExists();

            var mp3Files = Directory.GetFiles(mp3Folder, "*.mp3");

            var order = 10;
            foreach (var mp3File in mp3Files.SortNamesByChineseNumeral())
            {
                var fileInfo = new FileInfo(mp3File);
                var fileName = Path.GetFileNameWithoutExtension(mp3File);
                var txtFile = Path.Combine(txtFolder, $"{fileName}.txt");

                var mp3Info = await new Mp3Info(mp3File).Load();

                var chapter = new ChapterMeta
                {
                    Title = fileName,
                    Content = await ReadTextFile(txtFile),
                    FileLength = fileInfo.Length,
                    Duration = mp3Info.Duration.TotalMilliseconds,
                    Order = order++ * 10,
                    DateCreated = book.DateCreated,
                    DateUpdated = book.DateUpdated
                };
                meta.Episodes.Add(chapter);
            }

            _logger.LogInformation("Found {Count} chapters for book {Book}", meta.Episodes.Count, book.Title);

            await meta.WriteMetaToFile(path, (message) => { _logger.LogInformation(message); });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate metadata for book {Book} {Message}", book.Title, ex.FullMessage());
        }
    }

    private static Task<string> ReadTextFile(string path)
    {
        if (!File.Exists(path))
        {
            return Task.FromResult(string.Empty);
        }

        return File.ReadAllTextAsync(path);
    }
}
