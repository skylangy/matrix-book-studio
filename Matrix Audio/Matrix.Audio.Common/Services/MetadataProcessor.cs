using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Matrix.Audio.Common.Services;
public class MetadataProcessor(
    IOptions<AppConfiguration> appConfiguration,
    IEntityRepository entityRepository,
    ILogger<MetadataProcessor> logger) : IMetadataProcessor
{
    private const string ArtistMetaFile = "author_metadata.json";
    private const string AlbumMetaFile = "book_metadata.json";

    private readonly AppConfiguration _appConfiguration = appConfiguration.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<MetadataProcessor> _logger = logger;

    public async Task<int> ScanArtistMetadata()
    {
        var root = new DirectoryInfo(_appConfiguration.BooksLocation);
        _logger.LogInformation("Scanning artist metadata from {}", root.FullName);
        var artistMetaFiles = root.GetFiles(ArtistMetaFile, SearchOption.AllDirectories);

        return await ProcessArtistMetaFiles(artistMetaFiles);
    }

    public async Task<int> ScanAlbumMetadata()
    {
        var root = new DirectoryInfo(_appConfiguration.BooksLocation);
        _logger.LogInformation("Scanning album metadata from {}", root.FullName);
        var albumMetaFiles = root.GetFiles(AlbumMetaFile, SearchOption.AllDirectories);

        return await ProcessAlbumMetaFiles(albumMetaFiles);
    }

    private async Task<int> ProcessAlbumMetaFiles(FileInfo[] albumMetaFiles)
    {
        var result = 0;
        foreach (var file in albumMetaFiles)
        {
            _logger.LogInformation("Processing album metadata from {}", file.FullName);

            if (await ProcessAlbumMeta(file))
                result++;
        }
        return result;
    }

    private async Task<int> ProcessArtistMetaFiles(FileInfo[] artistMetaFiles)
    {
        var result = 0;
        foreach (var file in artistMetaFiles)
        {
            _logger.LogInformation("Processing artist metadata from {}", file.FullName);
            if (await ProcessArtistMeta(file))
                result++;
        }
        return result;
    }

    private async Task<bool> ProcessAlbumMeta(FileInfo metaFile)
    {
        var metadata = await metaFile.FullName.LoadAlbumMetaFromFile();
        if (metadata == null)
        {
            _logger.LogWarning("Album metadata is null for {}", metaFile.FullName);
            return false;
        }

        var album = await _entityRepository.GetAlbumByTitleAndArtist(metadata.Title!, metadata.Artist!);
        if (album != null && album.DateUpdated == metadata.DateUpdated)
        {
            _logger.LogInformation("Album {} by {} already up to date", metadata.Title, metadata.Artist);
            return false;
        }

        if (album == null)
        {
            album = metadata.FromMeta();
        }
        else
        {
            metadata.UpdateAlbum(album);
        }
        await UpdateAlbumProperties(album, metadata);
        await _entityRepository.UpdateAsync(album!);
        return true;
    }

    private async Task<bool> ProcessArtistMeta(FileInfo metaFile)
    {
        var metadata = await metaFile.FullName.LoadArtistMetaFromFile();
        if (metadata == null)
        {
            _logger.LogWarning("Artist metadata is null for {}", metaFile.FullName);
            return false;
        }

        var artist = await _entityRepository.GetArtistByName(metadata.Name!);
        if (artist != null && artist.DateUpdated == metadata.DateUpdated)
        {
            _logger.LogInformation("Artist {} already up to date", metadata.Name);
            return false;
        }

        if (artist == null)
        {
            artist = metadata.FromMeta();
        }
        else
        {
            metadata.UpdateArtist(artist);
        }
        await UpdateArtistProperties(artist, metadata);
        await _entityRepository.UpdateAsync(artist!);
        return true;
    }

    private async Task UpdateAlbumProperties(Album album, AlbumMeta metadata)
    {
        var artist = await GetCreateArtistFromName(metadata.Artist!);
        album.ArtistId = artist?.Id;

        var wideSplash = await GetOrCreateImageResource(metadata.ImageWideSplash!, @$"{metadata.Artist}\{metadata.Title}");
        var squareSplash = await GetOrCreateImageResource(metadata.ImageSquareSplash!, @$"{metadata.Artist}\{metadata.Title}");
        album.ImageWideSplashId = wideSplash?.Id;
        album.ImageSquareSplashId = squareSplash?.Id;

        album.Tags = await GetTagIdsByName(metadata.Tags!);
        album.Categories = await GetCategoryIdsByName(metadata.Categories!);
        album.Episodes = await ProcessEpisodeMeta(metadata.Episodes!, album);
    }

    private async Task UpdateArtistProperties(Artist artist, ArtistMeta metadata)
    {
        var imageResource = await GetOrCreateImageResource(metadata.Image!, metadata.Name!);
        artist.Image = imageResource.Id!;
    }

    private async Task<List<string>> ProcessEpisodeMeta(IEnumerable<EpisodeMeta> episodes, Album album)
    {
        var episodeIds = new HashSet<string>();
        foreach (var episodeMeta in episodes)
        {
            var episode = await _entityRepository.GetEpisodeByTitleAndAlbum(episodeMeta.Title!, album.Id);
            if (episode != null && episode.DateUpdated == episodeMeta.DateUpdated)
            {
                _logger.LogInformation("Episode {} by {} already up to date", episodeMeta.Title, album.Title);
                continue;
            }

            if (episode == null)
            {
                episode = episodeMeta.FromMeta();
                episode.AlbumId = album.Id;
            }
            else
            {
                episodeMeta.UpdateEpisode(episode);
            }

            await _entityRepository.UpdateAsync(episode!);
            episodeIds.Add(episode.Id);
        }
        return [.. episodeIds];
    }

    private async Task<Artist> GetCreateArtistFromName(string name)
    {
        var artist = await _entityRepository.GetArtistByName(name);
        if (artist != null)
        {
            return artist;
        }
        var artistFolder = Path.Combine(_appConfiguration.BooksLocation, name);
        var artistMetaFile = Path.Combine(artistFolder, ArtistMetaFile);

        if (File.Exists(artistMetaFile))
        {
            var metadata = await artistMetaFile.LoadArtistMetaFromFile();
            if (metadata != null)
            {
                return await CreateNewArtist(metadata);
            }
        }

        var imageResource = await GetOrCreateImageResource($"{name}.png", name);
        artist = new Artist
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Alias = name,
            Image = imageResource.Id!,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow
        };
        await _entityRepository.UpdateAsync(artist);
        return artist;
    }

    private async Task<Artist> CreateNewArtist(ArtistMeta metadata)
    {
        var imageResource = await GetOrCreateImageResource(metadata.Image!, metadata.Name!);

        var artist = new Artist
        {
            Id = Guid.NewGuid().ToString(),
            Name = metadata.Name!,
            Alias = metadata.Alias!,
            Description = metadata.Description!,
            Image = imageResource.Id!,
            DateCreated = metadata.DateCreated!,
            DateUpdated = metadata.DateUpdated!
        };
        await _entityRepository.UpdateAsync(artist);
        return artist;
    }

    private async Task<ImageResource> GetOrCreateImageResource(string fileName, string folderName)
    {
        var imageResource = await _entityRepository.GetImageByName(fileName, folderName);
        if (imageResource == null)
        {
            _logger.LogWarning("Image resource not found for {}", fileName);

            imageResource = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = fileName,
                FolderName = folderName
            };

            await _entityRepository.UpdateAsync(imageResource);

            return imageResource;
        }
        return imageResource;
    }

    private async Task<List<string>> GetTagIdsByName(IEnumerable<string> tagNames)
    {
        var tags = await GetTagsByName(tagNames);
        return tags.Select(t => t.Id).ToList();
    }

    private async Task<List<string>> GetCategoryIdsByName(IEnumerable<string> categoryNames)
    {
        var categories = await GetCategoriesByName(categoryNames);
        return categories.Select(c => c.Id).ToList();
    }

    private async Task<IEnumerable<Tag>> GetTagsByName(IEnumerable<string> tagNames)
    {
        if (tagNames == null || !tagNames.Any())
        {
            return [];
        }
        var tags = await _entityRepository.GetTagsByName(tagNames);
        return tags;
    }

    private async Task<IEnumerable<Category>> GetCategoriesByName(IEnumerable<string> categoryNames)
    {
        if (categoryNames == null || !categoryNames.Any())
        {
            return [];
        }
        var categories = await _entityRepository.GetCategoriesByName(categoryNames);
        return categories;
    }
}