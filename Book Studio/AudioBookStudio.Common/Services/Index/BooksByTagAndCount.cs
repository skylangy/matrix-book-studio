using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BooksByTagAndCount : AbstractIndexCreationTask<Book, BookGroupModel>
{
    public const string Unknown = nameof(Unknown);

    public BooksByTagAndCount()
    {
        Map = books => from book in books
                       from tagId in book.TagIds.DefaultIfEmpty(Unknown)
                       select new BookGroupModel()
                       {
                           Id = tagId == Unknown ? Unknown : tagId,
                           Name = tagId == Unknown ? Unknown : LoadDocument<Tag>(tagId).Name ?? Unknown,
                           Count = 1
                       };

        // Reduce function to group by tag and count the books
        Reduce = results => from result in results
                            group result by new { result.Id, result.Name } into g
                            select new BookGroupModel()
                            {
                                Id = g.Key.Id,
                                Name = g.Key.Name,
                                Count = g.Sum(x => x.Count)
                            };
    }
}
