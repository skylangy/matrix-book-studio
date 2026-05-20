using AudioBookStudio.Models.Data;
using DatabaseTools.Models;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Text.Encodings.Web;
using System.Text.Json;

using BookModels = AudioBookStudio.Models;
using Json = System.Text.Json;

namespace DatabaseTools.Commands;
public class BookToAudioCommand(
    ILogger<BookToAudioCommand> logger) : ICommand
{
    private readonly ILogger<BookToAudioCommand> _logger = logger;
    private readonly string _bookFolder = @"G:\Audio Books";
    private readonly JsonSerializerOptions _serializeOptions = new() { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };


    public string Name => "book-lib-to-audio-lib";
    public string Description => "Migrates data from SQLite to RavenDB";



    public async Task Execute(string[] args)
    {
        /*
         * Book -> Album
         * Author -> Artist
         * Category
         * Tag
         * ImageResources
         */
        var bookDb = new DocumentStore
        {
            Urls = ["http://localhost:8080"],
            Database = "MatrixBookLibrary",
            Conventions =
            {
                FindIdentityProperty = p => p.Name == "Id"
            }
        }.Initialize();

        var musicDb = new DocumentStore
        {
            Urls = ["http://localhost:8080"],
            Database = "MatrixAudio",
            Conventions =
            {
                FindIdentityProperty = p => p.Name == "Id"
            }
        }.Initialize();

        using var bookSession = bookDb.OpenAsyncSession();
        using var musicSession = musicDb.OpenAsyncSession();

        _logger.LogInformation("Loading data from Book database, Category, Tag, Image, Author, Book...");
        var categories = await bookSession.Query<BookModels.Data.Category>().ToListAsync();
        var tags = await bookSession.Query<BookModels.Data.Tag>().ToListAsync();
        var images = await bookSession.Query<BookModels.Data.ImageResource>().ToListAsync();
        var authors = await bookSession.Query<Author>().ToListAsync();
        var books = await bookSession.Query<Book>().Where(x => x.Status == BookStatus.Finished).ToListAsync();

        _logger.LogInformation("{} categories", categories.Count);
        _logger.LogInformation("{} tags", tags.Count);
        _logger.LogInformation("{} images", images.Count);
        _logger.LogInformation("{} authors", authors.Count);
        _logger.LogInformation("{} books\n", books.Count);


        _logger.LogInformation("Transforming Author to Artist...");
        var artists = authors.Select(x => new Artist
        {
            Id = x.Id,
            Name = x.Name,
            Alias = x.Alias,
            Description = x.Description,
            Image = x.Image,
            DateCreated = x.DateCreated ?? DateTime.Now,
            DateUpdated = x.DateUpdated ?? DateTime.Now
        }).ToList();

        _logger.LogInformation("Transforming Book to Album...");

        var episodes = new List<Episode>();
        foreach (var item in books.Select(x => new { x.Id, x.Author, x.Title }))
        {
            var albumPath = Path.Combine(_bookFolder, item.Author!, item.Title);
            var albumEpisodes = await LoadEpisods(item.Id, albumPath);
            episodes.AddRange(albumEpisodes);
        }
        _logger.LogInformation("Episodes loaded: {}", episodes.Count);

        var albums = books.Select(x => new Album
        {
            Id = x.Id,
            Title = x.Title,
            Subtitle = x.Subtitle,
            Description = x.Summary,
            ArtistId = x.AuthorId,
            Artist = x.Author,
            Status = AlbumStatus.Published,
            Categories = x.CategoryIds,
            Tags = x.TagIds,
            DateCreated = x.DateCreated ?? DateTime.Now,
            DateUpdated = x.DateUpdated ?? DateTime.Now,
            ImageWideSplashId = images.FirstOrDefault(i => i.FileName!.Contains($"{x.Title}-wide-bg.png"))?.Id,
            ImageSquareSplashId = images.FirstOrDefault(i => i.FileName!.Contains($"{x.Title}-square-bg.png"))?.Id,
            Episodes = episodes.Where(e => e.AlbumId == x.Id).Select(e => e.Id).ToList()

        }).ToList();


        // clear collections first
        _logger.LogInformation("Clearing collections: Tag, Category, Image, Author, Book...");
        await ClearCollection(musicSession, "Tags");
        await ClearCollection(musicSession, "Categories");
        await ClearCollection(musicSession, "ImagesResources");
        await ClearCollection(musicSession, "Artists");
        await ClearCollection(musicSession, "Episodes");
        await ClearCollection(musicSession, "Albums");

        // import to RavenDB
        _logger.LogInformation("Importing data to Music database...");

        await Import(categories, musicDb, $"Importing {categories.Count} Categories...");
        await Import(tags, musicDb, $"Importing {tags.Count} Tags...");
        await Import(images, musicDb, $"Importing {images.Count} Images...");
        await Import(artists, musicDb, $"Importing {artists.Count} Artists...");
        await Import(episodes, musicDb, $"Importing {episodes.Count} Episodes...");
        await Import(albums, musicDb, $"Importing {albums.Count} Albums...");

        _logger.LogInformation("Data imported to Music database");
    }

    private async Task<List<Episode>> LoadEpisods(string albumId, string bookFolder, bool reload = false)
    {
        if (string.IsNullOrWhiteSpace(albumId))
        {
            throw new ArgumentNullException(nameof(albumId));
        }

        var episodes = new List<Episode>();
        var txtFolder = Path.Combine(bookFolder, "txt");
        var mp3Folder = Path.Combine(bookFolder, "mp3");

        if (!Directory.Exists(mp3Folder))
        {
            _logger.LogWarning("Mp3 folder {} does not exist, will skip", mp3Folder);
            return episodes;
        }

        var albumFile = Path.Combine(mp3Folder, "album.json");

        if (!reload && File.Exists(albumFile))
        {
            _logger.LogInformation("Loading album from {}", albumFile);

            var json = File.ReadAllText(albumFile);
            episodes = Json.JsonSerializer.Deserialize<List<Episode>>(json);

            _logger.LogInformation("Albums load from file: {}", episodes!.Count);
        }
        else
        {
            _logger.LogWarning("Album file {} does not exist", albumFile);

            var files = Directory.GetFiles(mp3Folder, "*.mp3");
            _logger.LogInformation("\n\nLoading episodes from folder '{}' with '{}' files", mp3Folder, files.Length);

            foreach (var file in files)
            {
                try
                {
                    _logger.LogInformation("\tLoading episode from mp3 file {}", file);

                    var fileInfo = new FileInfo(file);
                    var title = Path.GetFileNameWithoutExtension(file);
                    var episode = new Episode
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlbumId = albumId,
                        Title = title,
                        Content = await GetEpisodeContent(txtFolder, title),
                        Duration = await GetMusicDuration(file),
                        DateCreated = fileInfo.CreationTime,
                        DateUpdated = fileInfo.LastWriteTime
                    };

                    episodes.Add(episode);

                    _logger.LogInformation("\tEpisode [{}] is loaded {}ms.", file, episode.Duration);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "\tFailed to load episode from '{}', {}", file, ex.Message);
                }
            }

            var json = Json.JsonSerializer.Serialize(episodes, _serializeOptions);
            File.WriteAllText(albumFile, json);
        }

        return episodes;
    }

    private async Task<string> GetEpisodeContent(string txtFolder, string title)
    {
        var file = Path.Combine(txtFolder, $"{title}.txt");
        _logger.LogInformation("\tLoading episode content from: {}", file);
        if (File.Exists(file))
        {
            return await File.ReadAllTextAsync(file);
        }

        return string.Empty;
    }

    private Task<double> GetMusicDuration(string mp3File)
    {
        if (!File.Exists(mp3File))
        {
            return Task.FromResult(0.0);
        }

        try
        {
            _logger.LogInformation("\tGetting music duration for {}", mp3File);

            using var mp3 = new Mp3FileReader(mp3File);
            var duration = mp3.TotalTime.TotalMilliseconds;

            _logger.LogInformation("\t{} duration: {} ms", mp3File, duration);

            return Task.FromResult(duration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "\tFailed to get duration for {}", mp3File);
            return Task.FromResult(0.0);
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

    private async Task Import<T>(IEnumerable<T> models, IDocumentStore documentStore, string message = "")
    {
        _logger.LogInformation("{}", message);
        using var bulkInsert = documentStore.BulkInsert();
        foreach (var model in models)
        {
            await bulkInsert.StoreAsync(model);
        }
    }
}
