using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BooksByStatusAndCount : AbstractIndexCreationTask<Book, BookGroupModel>
{
    public BooksByStatusAndCount()
    {
        Map = books => from book in books
                       select new BookGroupModel()
                       {
                           Id = book.Status,
                           Name = book.Status,
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
