using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using DatabaseTools.Models;
using DatabaseTools.Services;
using LiteDB;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Text.Json;
using BookModels = AudioBookStudio.Models;
using Json = System.Text.Json;

namespace DatabaseTools.Commands;
public class LiteDbToRavenDbCommand(
    IImageService imageService,
    ILogger<BookToAudioCommand> logger) : ICommand
{
    private readonly IImageService _imageService = imageService;
    private readonly ILogger<BookToAudioCommand> _logger = logger;

    private readonly string _bookFolder = @"G:\Audio Books";
    private readonly string _imageFolder = @"C:\Users\Andy\Documents\Books\My Books\封面\png";
    private readonly string _dbFolder = @"D:\Code\GitLab\matrix-book-studio\DatabaseTools\Data\";
    private readonly JsonSerializerOptions _deserializeOptions = new() { PropertyNameCaseInsensitive = true };

    public string Name => "lite-to-raven";
    public string Description => "Migrates data from SQLite to RavenDB";


    public async Task Execute(string[] args)
    {
        var bookDb = Path.Combine(_dbFolder, "books.db");
        var regexDb = Path.Combine(_dbFolder, "regex.db");
        var optionsDb = Path.Combine(_dbFolder, "options.db");

        var bookLiteDb = new LiteDatabase(bookDb);
        var optionsLiteDb = new LiteDatabase(optionsDb);
        var regexLiteDb = new LiteDatabase(regexDb);

        var ravenDb = new DocumentStore
        {
            Urls = ["http://localhost:8080"],
            Database = "MatrixBookLibrary",
            Conventions =
            {
                FindIdentityProperty = p => p.Name == "Id"
            }
        }.Initialize();

        _logger.LogInformation("Loading books...");

        // load old models
        var oldBooks = bookLiteDb.GetCollection<OldBook>("books")
                            .FindAll();
        var authors = bookLiteDb.GetCollection<Author>("authors")
                            .FindAll()
                            .DistinctBy(x => x.Name!.Trim())
                            .ToList();
        var options = optionsLiteDb.GetCollection<Option>("options")
                            .FindAll()
                            .Select(x =>
                            {
                                var valueWrapper = Json.JsonSerializer.Deserialize<ValueWrapper>(x.Value!, _deserializeOptions);
                                return new Option
                                {
                                    Id = x.Id ?? Guid.NewGuid().ToString(),
                                    Name = x.Name ?? x.DisplayName!.Replace(" ", ""),
                                    DisplayName = x.DisplayName,
                                    Group = x.Group,
                                    ValueType = x.ValueType,
                                    Value = valueWrapper!.Value,
                                };
                            })
                            .ToList();
        var regexes = regexLiteDb.GetCollection<RegexModel>("regexes")
                            .FindAll()
                            .ToList();

        _logger.LogInformation("Loading images...");
        // transform to new models: images, categories, tags, authors, books
        var images = await PrepareImages(oldBooks, _bookFolder, _imageFolder);
        PrepareAuthorImages(authors, images);

        _logger.LogInformation("Loading categories...");
        var categories = oldBooks.Where(x => !string.IsNullOrWhiteSpace(x.Category))
                         .SelectMany(x => x.Category!.Split(','))
                         .Select(x => x.Trim())
                         .Distinct()
                         .Select(x => new BookModels.Data.Category { Id = Guid.NewGuid().ToString(), Name = x })
                         .ToList();

        _logger.LogInformation("Loading tags...");
        var tags = oldBooks.Where(x => !string.IsNullOrWhiteSpace(x.Tag))
                        .SelectMany(x => x.Tag!.Split(','))
                        .Select(x => x.Trim())
                        .Distinct()
                        .Select(x => new BookModels.Data.Tag { Id = Guid.NewGuid().ToString(), Name = x })
                        .ToList();

        _logger.LogInformation("Transforming authors...");
        authors.ForEach(author =>
        {
            var image = images.FirstOrDefault(i => i.FolderName == author.Name);
            if (image != null)
            {
                author.Image = image.Id;
                author.ImageIds.Clear();
                author.ImageIds.Add(image.Id);
            }
        });

        _logger.LogInformation("Transforming books...");
        var books = oldBooks.Select(x =>
        {
            var bookSplashName = x.Images?.FirstOrDefault()?.Name;
            var bookSplashUrl = x.Images?.FirstOrDefault()?.Url;
            var bookImages = images.Where(i => i.FileName == bookSplashUrl).ToList();

            var book = new Book
            {
                Id = x.Id ?? Guid.NewGuid().ToString(),
                Title = x.Title,
                Subtitle = x.Subtitle,
                Summary = x.Summary ?? GetSummary(x.Content),
                Content = x.Content,
                Author = x.Author,
                AuthorId = GetAuthorId(x.Author!, authors),
                Status = x.Status,
                TagIds = GetTagIds(x.Tag, tags),
                CategoryIds = GetCategoryIds(x.Category, categories),
                Year = x.Year,
                DefaultImageId = string.IsNullOrWhiteSpace(bookSplashUrl) ? bookImages.FirstOrDefault()?.Id : images.FirstOrDefault(i => i.FileName == bookSplashUrl)?.Id,
                Language = x.Language,
                VoiceName = x.VoiceName,
                WavGenerated = x.WavGenerated,
                Mp3Generated = x.Mp3Generated,
                Mp4Generated = x.Mp4Generated,
                SrtGenerated = x.SrtGenerated,
                TextGenerated = x.TextGenerated,
                Hide = x.Hide,
                PublishOrder = x.PublishOrder,
                Rank = x.Rank,
                TextCount = x.TextCount,
                DateCreated = x.DateCreated ?? DateTime.Now,
                DateUpdated = x.DateUpdated ?? DateTime.Now,
                ImageIds = bookImages.Select(i => i.Id).ToList(),
            };

            return book;
        }).ToList();

        _logger.LogInformation("Clear entity collection...");
        using var session = ravenDb.OpenAsyncSession();
        await ClearCollection(session, "Tags");
        await ClearCollection(session, "Categories");
        await ClearCollection(session, "ImaggeResources");
        await ClearCollection(session, "Authors");
        await ClearCollection(session, "Books");

        // import to RavenDB
        _logger.LogInformation("Importing data to RavenDB...");
        await Import(categories, ravenDb);
        await Import(tags, ravenDb);
        await Import(images, ravenDb);
        await Import(authors, ravenDb);
        await Import(books, ravenDb);
        await Import(options, ravenDb);
        await Import(regexes, ravenDb);
    }

    private static List<string> GetTagIds(string? rawTag, List<BookModels.Data.Tag> tags)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
        {
            return [];
        }


        var tagNames = rawTag.Split(',').Select(x => x.Trim()).Distinct().ToHashSet();

        return tags.Where(t => tagNames.Contains(t.Name!.Trim()))
                   .Select(x => x.Id!)
                   .ToList();
    }

    private static List<string> GetCategoryIds(string? rawCategory, List<BookModels.Data.Category> categories)
    {
        if (string.IsNullOrWhiteSpace(rawCategory))
        {
            return [];
        }
        var categoryNames = rawCategory.Split(',').Select(x => x.Trim()).Distinct().ToHashSet();
        return categories.Where(c => categoryNames.Contains(c.Name!.Trim()))
                         .Select(x => x.Id!)
                         .ToList();
    }

    private static string GetAuthorId(string authorName, List<Author> authors)
    {
        var author = authors.FirstOrDefault(x => x.Name!.Trim() == authorName.Trim());

        if (author == null)
        {
            author = new Author
            {
                Id = Guid.NewGuid().ToString(),
                Name = authorName.Trim(),
                Alias = authorName.Trim(),
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now
            };
            authors.Add(author);
        }
        return author.Id;
    }

    private static string GetSummary(string? content, int length = 400)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return string.Empty;
        }

        var summary = content.Length > length ? content[..length] : content;
        return summary.Trim();
    }

    private Task<List<ImageResource>> PrepareImages(IEnumerable<OldBook> books, string bookFolder, string imagesFolder)
    {
        const string exension = "png";
        var imageResources = new List<ImageResource>();
        var images = Directory.GetFiles(imagesFolder, "*.png");

        foreach (var book in books)
        {
            if (string.IsNullOrWhiteSpace(book.Title) || string.IsNullOrWhiteSpace(book.Author))
                continue;

            var bookName = book.Title.Trim();
            var author = book.Author.Trim();
            var imageList = new List<string>();

            var bookImageFolder = Path.Combine(bookFolder, author, bookName, "images");
            bookImageFolder.EnsureDirectoryExists();

            var bookSplashes = Directory.GetFiles(bookImageFolder, "*.*")
                                        .Where(file => file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                                       file.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                                        .ToList();


            var wideSplash = images.FirstOrDefault(x => x.Contains($"{bookName}-wide-splash.{exension}"));
            if (wideSplash != null)
            {
                imageList.Add(wideSplash);
            }

            var wideBg = images.FirstOrDefault(x => x.Contains($"{bookName}-wide-bg.{exension}"));
            if (wideBg != null)
            {
                imageList.Add(wideBg);
            }

            var squareSplash = images.FirstOrDefault(x => x.Contains($"{bookName}-square-splash.{exension}"));
            if (squareSplash != null)
            {
                imageList.Add(squareSplash);
            }

            var squareBg = images.FirstOrDefault(x => x.Contains($"{bookName}-square-bg.{exension}"));
            if (squareBg != null)
            {
                imageList.Add(squareBg);
            }

            foreach (var image in book.Images!)
            {
                var imgFile = images.FirstOrDefault(x => Path.GetFileName(x) == image.Url);
                if (imgFile != null)
                {
                    imageList.Add(imgFile);
                }
            }

            foreach (var image in bookSplashes)
            {
                imageList.Add(image);
            }

            foreach (var image in imageList)
            {
                var fileName = Path.GetFileName(image);
                var dest = Path.Combine(bookImageFolder, fileName);

                if (!File.Exists(dest))
                {
                    _logger.LogInformation("Copying image {} to {}", image, dest);
                    File.Copy(image, dest, true);
                }

                var imageInfo = _imageService.GetImageInfo(image);
                var imageResource = new ImageResource
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = fileName,
                    FolderName = Path.Combine(author, bookName),
                    Width = imageInfo.Width,
                    Height = imageInfo.Height,
                };
                imageResources.Add(imageResource);
            }
        }

        return Task.FromResult(imageResources);
    }

    private void PrepareAuthorImages(IEnumerable<Author> authors, List<ImageResource> imageResources)
    {
        foreach (var author in authors.Where(x => !string.IsNullOrWhiteSpace(x.Image))
                                      .DistinctBy(x => x.Name!.Trim()))
        {
            var imageFolder = Path.Combine(_bookFolder, author.Name!, "images");
            var image = Path.Combine(imageFolder, $"{author.Name!.Trim()}.jpg");
            var imageInfo = _imageService.GetImageInfo(image);

            var resource = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = author.Image!,
                FolderName = author.Name,
                Width = imageInfo.Width,
                Height = imageInfo.Height
            };
            imageResources.Add(resource);
        }
    }

    private static async Task Import<T>(IEnumerable<T> models, IDocumentStore documentStore)
    {
        using var bulkInsert = documentStore.BulkInsert();
        foreach (var model in models)
        {
            await bulkInsert.StoreAsync(model);
        }
    }

    private async Task ClearCollection(IAsyncDocumentSession session, string collectionName)
    {
        try
        {
            await session.Advanced.AsyncRawQuery<object>($"from {collectionName} delete").ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear collection: {}", collectionName);
        }
    }


    class ValueWrapper
    {
        public required int Kind { get; set; }
        public required string Value { get; set; }
    }
}
