using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BookByStatus : AbstractIndexCreationTask<Book>

{
    public BookByStatus()
    {
        Map = books => from book in books
                       select new
                       {
                           book.Status,
                           book.Title,
                           book.Author
                       };
    }
}
