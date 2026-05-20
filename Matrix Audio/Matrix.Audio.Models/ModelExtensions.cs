using System.Text.Encodings.Web;
using System.Text.Json;

namespace Matrix.Audio.Models;
public static class ModelExtensions
{
    public static Album FromMeta(this AlbumMeta meta) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Title = meta.Title,
        Subtitle = meta.Subtitle,
        Description = meta.Description,
        Artist = meta.Artist,
        DateCreated = meta.DateCreated,
        DateUpdated = meta.DateUpdated
    };

    public static void UpdateAlbum(this AlbumMeta meta, Album album)
    {
        album.Title = meta.Title;
        album.Subtitle = meta.Subtitle;
        album.Description = meta.Description;
        album.Artist = meta.Artist;
        album.DateUpdated = meta.DateUpdated;
    }

    public static Artist FromMeta(this ArtistMeta meta) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Name = meta.Name,
        Alias = meta.Alias,
        Description = meta.Description,
        DateCreated = meta.DateCreated,
        DateUpdated = meta.DateUpdated
    };

    public static void UpdateArtist(this ArtistMeta meta, Artist artist)
    {
        artist.Name = meta.Name;
        artist.Alias = meta.Alias;
        artist.Description = meta.Description;
        artist.DateUpdated = meta.DateUpdated;
    }

    public static Episode FromMeta(this EpisodeMeta meta) => new()
    {
        Id = Guid.NewGuid().ToString(),
        AlbumId = string.Empty,
        Title = meta.Title,
        Content = meta.Content,
        Duration = meta.Duration,
        FileLength = meta.FileLength,
        Order = meta.Order,
        DateCreated = meta.DateCreated,
        DateUpdated = meta.DateUpdated
    };

    public static void UpdateEpisode(this EpisodeMeta meta, Episode episode)
    {
        episode.Title = meta.Title;
        episode.Content = meta.Content;
        episode.Duration = meta.Duration;
        episode.FileLength = meta.FileLength;
        episode.Order = meta.Order;
        episode.DateUpdated = meta.DateUpdated;
    }

    public static JsonSerializerOptions JsonOptions => new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, JsonOptions);

    public static async Task<AlbumMeta?> LoadAlbumMetaFromFile(this string path)
    {
        var content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }
        return JsonSerializer.Deserialize<AlbumMeta>(content, JsonOptions);
    }

    public static async Task<ArtistMeta?> LoadArtistMetaFromFile(this string path)
    {
        var content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }
        return JsonSerializer.Deserialize<ArtistMeta>(content, JsonOptions);
    }
}
