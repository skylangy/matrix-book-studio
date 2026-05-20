using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class LibrarySyncer(
    IOptions<AppConfiguration> appConfig,
    IEntityRepository entityRepository,
    IFileTransfer fileTransfer,
    ILogger<LibrarySyncer> logger) : ILibrarySyncer
{
    private readonly AppConfiguration _appConfig = appConfig.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IFileTransfer _fileTransfer = fileTransfer;
    private readonly ILogger<LibrarySyncer> _logger = logger;

    public async Task<ResultBase> SyncAuthor(string authorId)
    {
        if (string.IsNullOrWhiteSpace(authorId))
        {
            return ResultBase.Fail("Author ID cannot be null or empty");
        }

        var author = await _entityRepository.GetAsync<Author>(authorId);
        if (author == null)
        {
            return ResultBase.Fail("Author not found");
        }

        var authorFolder = Path.Combine(_appConfig.BooksLocation, author.Name);
        var metadataFile = Path.Combine(authorFolder, MetadataConsts.AuthorMetaFile);
        var imagesFolder = Path.Combine(authorFolder, "images");

        _logger.LogInformation("{Transfer} Syncing author {AuthorName} with ID {AuthorId}", _fileTransfer.GetType().Name, author.Name, author.Id);
        if (File.Exists(metadataFile))
        {
            await _fileTransfer.Transfer(metadataFile, author.Name);
        }

        if (Directory.Exists(imagesFolder))
        {
            await _fileTransfer.Transfer(imagesFolder, author.Name);
        }

        return ResultBase.OK();
    }

    public async Task<ResultBase> SyncBook(string bookId)
    {
        var book = await _entityRepository.GetAsync<Book>(bookId);
        if (book == null)
        {
            return ResultBase.Fail("Book not found");
        }

        var bookFolder = Path.Combine(_appConfig.BooksLocation, book.Author, book.Title);
        var imagesFolder = Path.Combine(bookFolder, "images");
        var mp3Folder = Path.Combine(bookFolder, "mp3");
        var metadatFile = Path.Combine(bookFolder, MetadataConsts.AlbumMetaFile);

        _logger.LogInformation("{Transfer} Syncing author {BookAuthor} with ID {BookTitle}", _fileTransfer.GetType().Name, book.Author, book.Title);
        if (File.Exists(metadatFile))
        {
            await _fileTransfer.Transfer(metadatFile, Path.Combine(book.Author, book.Title));
        }

        if (Directory.Exists(imagesFolder))
        {
            await _fileTransfer.Transfer(imagesFolder, Path.Combine(book.Author, book.Title));
        }

        if (Directory.Exists(mp3Folder))
        {
            await _fileTransfer.Transfer(mp3Folder, Path.Combine(book.Author, book.Title));
        }

        return ResultBase.OK();
    }
}
