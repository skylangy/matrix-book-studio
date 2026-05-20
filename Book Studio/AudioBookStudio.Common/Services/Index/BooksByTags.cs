using Raven.Client.Documents.Indexes;

namespace AudioBookStudio.Common.Services.Index;
public class BooksByTags : AbstractIndexCreationTask<Book>
{
    public BooksByTags()
    {
        Map = books => from book in books
                       select new
                       {
                           book.TagIds
                       };
    }
}
