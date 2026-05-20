using AudioBookStudio.Models.Data;
using MatrixBook.Server.Models;

namespace MatrixBook.Server.Common;

public static class BookExtensions
{
    public static IQueryable<Book> Select(this IQueryable<Book> books)
    {
        return books.Select(x => new Book
        {
            Id = x.Id,
            Title = x.Title,
            Subtitle = x.Subtitle,
            Summary = x.Summary ?? x.Content!.Substring(0, 100),
            Content = x.Content!.Substring(0, 200),
            Author = x.Author,
            Status = x.Status,
            CategoryIds = x.CategoryIds,
            TagIds = x.TagIds,
            Year = x.Year,
            DefaultImageId = x.DefaultImageId,
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
            DateUpdated = x.DateUpdated,
            DateCreated = x.DateCreated,
            ImageIds = x.ImageIds
        });
    }

    public static SpeechConfigModel ToModel(this SpeechConfiguration config)
    {
        return new SpeechConfigModel
        {
            Services = [.. config.Services.Select(service => new SpeechServiceConfigModel
            {
                Name = service.Name,
                Language = service.Language,
                VoiceName = service.VoiceName,
                Languages = [.. service.Languages.Select(language => new SpeechLanguageModel
                {
                    Name = language.Name,
                    Value = language.Value,
                    Voices = [.. language.Voices.Select(voice => new SpeechVoiceModel
                    {
                        Name = voice.Name,
                        Value = voice.Value,
                        Tag = voice.Tag
                    })]
                })]
            })]
        };
    }
}
