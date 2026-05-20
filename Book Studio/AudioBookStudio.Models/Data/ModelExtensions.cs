using System.Text.Encodings.Web;
using System.Text.Json;

namespace AudioBookStudio.Models.Data;

public static class ModelExtensions
{
    public static AuthorMeta ToMeta(this Author author) => new()
    {
        Name = author.Name,
        Alias = author.Alias,
        Description = author.Description,
        Image = author.Image,
        DateCreated = author.DateCreated,
        DateUpdated = author.DateUpdated
    };

    public static BookMeta ToMeta(this Book book) => new()
    {
        Title = book.Title,
        Subtitle = book.Subtitle,
        Description = book.Summary,
        Artist = book.Author,
        ImageWideSplash = "",
        ImageSquareSplash = "",
        Episodes = [],
        Tags = [],
        Categories = [],
        DateCreated = book.DateCreated,
        DateUpdated = book.DateUpdated
    };

    public static JsonSerializerOptions JsonOptions => new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, JsonOptions);

    public static Task WriteMetaToFile(this BookMeta meta, string path, Action<string>? logAction = null)
    {
        var json = meta.ToJson();
        logAction?.Invoke($"Writing metadata to {path}");

        try
        {
            return File.WriteAllTextAsync(path, json);
        }
        catch (Exception ex)
        {
            logAction?.Invoke($"Failed to write metadata to {path}: {ex.Message}");
            return Task.FromException(ex);
        }
    }

    public static Task WriteMetaToFile(this AuthorMeta meta, string path)
    {
        var json = meta.ToJson();
        return File.WriteAllTextAsync(path, json);
    }

    public static async Task<BookMeta?> LoadBookMetaFromFile(this string path)
    {
        var content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }
        return JsonSerializer.Deserialize<BookMeta>(content, JsonOptions);
    }

    public static async Task<AuthorMeta?> LoadAuthorMetaFromFile(this string path)
    {
        var content = await File.ReadAllTextAsync(path);
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }
        return JsonSerializer.Deserialize<AuthorMeta>(content, JsonOptions);
    }
}
