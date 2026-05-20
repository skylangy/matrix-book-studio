using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BooksByAuthorAndCount : AbstractIndexCreationTask<Book, BookGroupModel>
{
    public BooksByAuthorAndCount()
    {
        Map = books => from book in books
                       select new BookGroupModel()
                       {
                           Id = book.Author,
                           Name = book.Author,
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