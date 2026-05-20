namespace AudioBookStudio.Common.Abstracts;
public interface IMetadataProcessor
{
    Task GenerateBookMeta(Book book);

    Task GenerateAuthorMeta(Author author);
}
