using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Common.Shared;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Models;
using MatrixBook.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthorController(
    IOptions<AppConfiguration> bookConfig,
    IEntityRepository entityRepository,
    IMetadataProcessor metadataProcessor,
    IBookExportService bookExportService,
    ILibrarySyncer librarySyncer,
    ILogger<BookController> logger) : ControllerBase
{
    private readonly AppConfiguration _bookConfig = bookConfig.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IMetadataProcessor _metadataProcessor = metadataProcessor;
    private readonly IBookExportService _bookExportService = bookExportService;
    private readonly ILibrarySyncer _librarySyncer = librarySyncer;
    private readonly ILogger<BookController> _logger = logger;


    [HttpGet("all", Name = "allAuthors")]
    public async Task<IEnumerable<Author>> Get()
    {
        var authors = await _entityRepository.GetAllAsync<Author>();

        return authors.OrderBy(x => x.DateUpdated);
    }

    [HttpGet("paged/{page}/{pageSize}", Name = "pagedAuthors")]
    public async Task<IPagedList<AuthorViewModel>> GetAuthors(int page = 1, int pageSize = 12)
    {
        var total = await _entityRepository.CountAsync<Author>(session => session.Query<Author>());

        var authors = await _entityRepository.QueryAsync(session => session.Query<Author>()
                                                                          .OrderByDescending(x => x.DateUpdated)
                                                                          .Skip((page - 1) * pageSize)
                                                                          .Take(pageSize));

        async Task<int> bookCount(Author author) =>
           await _entityRepository.CountAsync(session => session.Query<Book>().Where(x => x.Author == author.Name));

        var result = await Task.WhenAll(authors.Select(async x => new AuthorViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Alias = x.Alias,
            Description = x.Description,
            Image = x.Image,
            Books = await bookCount(x),
            DateCreated = x.DateCreated,
            DateUpdated = x.DateUpdated
        }));

        return result.ToPagedList(page, pageSize, total);
    }

    [HttpGet("{id}", Name = "getAuthor")]
    public async Task<Author?> Get(string id)
    {
        var author = await _entityRepository.GetAsync<Author>(id);

        return author;
    }

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchAuthors")]
    public async Task<IPagedList<AuthorViewModel>> Search(string keyword, int page, int pageSize)
    {
        var total = await _entityRepository.CountAsync(session =>
        {
            var query = session.Query<Author>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Name, keyword);
            }
            return query;
        });

        var authors = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Author>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Name, keyword);
            }
            return query.OrderByDescending(x => x.DateUpdated)
                         .Skip((page - 1) * pageSize)
                         .Take(pageSize);
        });

        async Task<int> bookCount(Author author) =>
           await _entityRepository.CountAsync(session => session.Query<Book>().Where(x => x.Author == author.Name));
        var result = await Task.WhenAll(authors.Select(async x => new AuthorViewModel
        {
            Id = x.Id,
            Name = x.Name,
            Alias = x.Alias,
            Description = x.Description,
            Image = x.Image,
            Books = await bookCount(x),
            DateCreated = x.DateCreated,
            DateUpdated = x.DateUpdated
        }));

        return result.ToPagedList(page, pageSize, total);
    }

    [HttpPost("", Name = "createAuthor")]
    public async Task<Author> Create([FromBody] Author author)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(author.Name);

        author.Id = Guid.NewGuid().ToString();
        author.DateCreated = DateTime.Now;
        author.DateUpdated = DateTime.Now;

        var name = author.Name.Trim();
        var existing = await _entityRepository.GetAuthorByNameAsync(name);
        if (existing != null)
        {
            return existing;
        }

        await _entityRepository.UpdateAsync(author);

        var authorFolder = Path.Combine(_bookConfig.BooksLocation, name);
        var imageFolder = Path.Combine(authorFolder, "images");
        authorFolder.EnsureDirectoryExists();
        imageFolder.EnsureDirectoryExists();

        return author;
    }

    [HttpPut("update", Name = "updateAuthor")]
    public async Task<bool> Update([FromBody] Author author)
    {
        _logger.LogInformation("Updating author {}", author.Name);
        author.Name = author.Name!.Trim();
        author.DateUpdated = DateTime.Now;

        var existing = await _entityRepository.GetByIdAsync<Author>(author.Id);
        existing ??= await _entityRepository.GetAuthorByNameAsync(author.Name);
        if (existing != null && existing.Name != author.Name)
        {
            try
            {
                var avatarFile = Path.Combine(_bookConfig.BooksLocation, existing.Name!, "images", $"{existing.Name}{ResourceTypes.SplashType}");
                var newAvatar = Path.Combine(_bookConfig.BooksLocation, existing.Name!, "images", $"{author.Name}{ResourceTypes.SplashType}");
                if (System.IO.File.Exists(avatarFile))
                {
                    System.IO.File.Move(avatarFile, newAvatar);
                }

                var folder = Path.Combine(_bookConfig.BooksLocation, existing.Name!);
                var destFolder = Path.Combine(_bookConfig.BooksLocation, author.Name!);
                var destImageFolder = Path.Combine(destFolder, "images");
                if (Directory.Exists(folder) && !Directory.Exists(destFolder))
                {
                    Directory.Move(folder, destFolder);
                }

                destFolder.EnsureDirectoryExists();
                destImageFolder.EnsureDirectoryExists();

                if (!string.IsNullOrWhiteSpace(existing.Image))
                {
                    var imageResource = await _entityRepository.GetByIdAsync<ImageResource>(existing.Image);
                    imageResource.FolderName = author.Name;
                    imageResource.FileName = $"{author.Name}{ResourceTypes.SplashType}";
                    await _entityRepository.UpdateAsync(imageResource);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming author folder");
            }
        }

        var result = await _entityRepository.UpdateAsync(author);
        var authorFolder = Path.Combine(_bookConfig.BooksLocation, author!.Name);
        var imageFolder = Path.Combine(authorFolder, "images");
        authorFolder.EnsureDirectoryExists();
        imageFolder.EnsureDirectoryExists();

        await _metadataProcessor.GenerateAuthorMeta(author);
        return result;
    }

    [HttpPost("clear/image/{id}", Name = "clearImage")]
    public async Task<bool> ClearImage(string id)
    {
        var author = await _entityRepository.GetAsync<Author>(id);
        author.Image = string.Empty;
        author.ImageIds.Clear();
        var imageFolder = Path.Combine(_bookConfig.BooksLocation, author.Name!, "images");
        if (Directory.Exists(imageFolder))
        {
            Directory.Delete(imageFolder, true);
        }
        var result = await _entityRepository.UpdateAsync(author);
        return result;
    }

    [HttpDelete("{id}", Name = "deleteAuthor")]
    public async Task<bool> Delete(string id)
    {
        return await _entityRepository.DeleteAsync<Author>(id);
    }

    [HttpPost("openAuthorFolder", Name = "openAuthorFolder")]
    public async Task OpenBookFolder([FromBody] OpenBookModel model)
    {
        _logger.LogInformation("Open book folder with id: {}", model.Id);

        var author = await _entityRepository.GetAsync<Author>(model.Id);
        if (author != null)
        {
            var folder = Path.Combine(_bookConfig.BooksLocation, author.Name!);
            folder.EnsureDirectoryExists();

            var imageFolder = Path.Combine(folder, "images");
            imageFolder.EnsureDirectoryExists();

            await _bookExportService.OpenBookFolder(folder);
        }
    }

    [HttpPost("publish/library/{authorId}", Name = "publishAuthorToLibrary")]
    public async Task<ResultBase> PublishToLibrary(string authorId)
    {
        return await _librarySyncer.SyncAuthor(authorId);
    }

    [HttpPost("publish/library/all", Name = "publishAllAuthorToLibrary")]
    public async Task<IEnumerable<ResultBase>> PublishAllToLibrary()
    {
        var authors = await _entityRepository.GetAllAsync<Author>();
        var authorIds = authors.Select(x => x.Id).ToList();
        return await _librarySyncer.SyncAuthors(authorIds);
    }
}
