
using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;

public class BooksByCategories : AbstractIndexCreationTask<Book>
{
    public BooksByCategories()
    {
        Map = books => from book in books
                       select new
                       {
                           book.CategoryIds
                       };
    }
}
