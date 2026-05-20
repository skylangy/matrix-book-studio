using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BooksByCategoryAndCount : AbstractIndexCreationTask<Book, BookGroupModel>
{
    public const string Unknown = nameof(Unknown);

    public BooksByCategoryAndCount()
    {
        Map = books => from book in books
                       from categoryId in book.CategoryIds.DefaultIfEmpty(Unknown)
                       select new BookGroupModel()
                       {
                           Id = categoryId == Unknown ? Unknown : categoryId,
                           Name = categoryId == Unknown ? Unknown : LoadDocument<Category>(categoryId).Name ?? Unknown,
                           Count = 1
                       };

        // Reduce function to group by category and count the books
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
