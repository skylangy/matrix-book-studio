namespace AudioBookStudio.Common.Abstracts;
public interface ILibrarySyncer
{
    Task<ResultBase> SyncBook(string bookId);

    Task<ResultBase> SyncAuthor(string authorId);
}
