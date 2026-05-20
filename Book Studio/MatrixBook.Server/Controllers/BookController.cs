using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Common.Services.Index;
using AudioBookStudio.Common.Shared;
using AudioBookStudio.Models.Common;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Common;
using MatrixBook.Server.Models;
using MatrixBook.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System.Linq.Expressions;
using System.Text;
using IO = System.IO;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BookController(
    IOptions<AppConfiguration> bookConfig,
    IOptions<SpeechConfiguration> speechConfig,
    IEntityRepository entityRepository,
    IBookExportService bookExportService,
    IWorkProgressRepository workProgressRepository,
    IMetadataProcessor metadataProcessor,
    ILibrarySyncer librarySyncer,
    ILogger<BookController> logger) : ControllerBase
{
    private readonly ILogger<BookController> _logger = logger;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly AppConfiguration _bookConfig = bookConfig.Value;
    private readonly SpeechConfiguration _speechConfig = speechConfig.Value;
    private readonly IBookExportService _bookExportService = bookExportService;
    private readonly IWorkProgressRepository _workProgressRepository = workProgressRepository;
    private readonly IMetadataProcessor _metadataProcessor = metadataProcessor;
    private readonly ILibrarySyncer _librarySyncer = librarySyncer;

    private const float MaxOrder = 1000;

    [HttpGet("all", Name = "getAll")]
    public async Task<IEnumerable<Book>> Get()
    {
        return await _entityRepository.GetAllAsync<Book>();
    }

    [HttpGet("{id}", Name = "getBook")]
    public async Task<Book?> Get(string id)
    {
        return await _entityRepository.GetAsync<Book>(id);
    }

    [HttpGet("recent/{count}/{keyword?}", Name = "getRecentBooks")]
    public async Task<IEnumerable<Book>> GetBooksByCategory(int count = 30, string? keyword = "")
    {
        _logger.LogInformation("Get recent books with keyword: '{}', '{}'", keyword, count);
        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>()
                               .Where(x => x.Status == BookStatus.InProgress)
                               .OrderByDescending(x => x.Rank)
                               .ThenByDescending(x => x.DateUpdated);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword, options: SearchOptions.Or)
                             .Search(x => x.Author, keyword, options: SearchOptions.Or);

                //.Search(x => x.Content, keyword, options: SearchOptions.Or) // could slow down the query a lot
            }

            LogQuery<Book>(session, query);

            return query.Take(count);
        });
        return books.ExcludeContent();
    }

    [HttpGet("name/{name}", Name = "getBookByName")]
    public async Task<Book?> GetBookByName(string name)
    {
        _logger.LogInformation("Get book by name: '{}'", name);
        return await _entityRepository.GetByNameAsync(name);
    }

    [HttpGet("id/{id}", Name = "getBookById")]
    public async Task<Book?> GetBookById(string id)
    {
        _logger.LogInformation("Get book by id: '{}'", id);
        return await _entityRepository.GetByIdAsync<Book>(id);
    }

    [HttpGet("group/categories", Name = "getBookCategories")]
    public async Task<IEnumerable<BookGroupModel>> GetBookCategories()
    {
        _logger.LogInformation("Get all categories");
        var groups = await _entityRepository.QueryAsync(session => session.Query<BookGroupModel, BooksByCategoryAndCount>().OrderByDescending(x => x.Count));

        return groups;
    }

    [HttpGet("group/tags", Name = "getBookTags")]
    public async Task<IEnumerable<BookGroupModel>> GetBookTags()
    {
        _logger.LogInformation("Get all tags");
        var groups = await _entityRepository.QueryAsync(session => session.Query<BookGroupModel, BooksByTagAndCount>().OrderByDescending(x => x.Count));

        return groups;
    }

    [HttpGet("group/authors", Name = "getBookAuthors")]
    public async Task<IEnumerable<BookGroupModel>> GetBookAuthors()
    {
        _logger.LogInformation("Get all authors");
        var groups = await _entityRepository.QueryAsync(session => session.Query<BookGroupModel, BooksByAuthorAndCount>().OrderByDescending(x => x.Count));
        return groups;
    }

    [HttpGet("group/status", Name = "getBookStatus")]
    public async Task<IEnumerable<BookGroupModel>> GetBookStatus()
    {
        _logger.LogInformation("Get all status");
        var groups = await _entityRepository.QueryAsync(session => session.Query<BookGroupModel, BooksByStatusAndCount>().OrderByDescending(x => x.Count));

        return groups;
    }

    [HttpGet("finished", Name = "getFinishedBooks")]
    public async Task<IEnumerable<BookInfo>> GetFinishedBooks()
    {
        var books = await _entityRepository.QueryAsync(session => session.Query<Book>()
                                                                         .Where(x => x.Status == BookStatus.Finished)
                                                                         .OrderByDescending(x => x.DateUpdated));

        return books.Select(x => new BookInfo
        {
            Author = x.Author,
            DateCreated = x.DateCreated,
            DateUpdated = x.DateUpdated,
            Id = x.Id,
            Status = x.Status,
            Subtitle = x.Subtitle,
            TextCount = x.TextCount,
            Title = x.Title
        });
    }

    [HttpGet("byTag/{tagName}/{page}/{pageSize}/{inProgressOnly}", Name = "getBooksByTag")]
    public async Task<IPagedList<Book>> GetGroupedBooksByTag(string tagName, int page = 1, int pageSize = 30, bool inProgressOnly = false)
    {
        _logger.LogInformation("Get books by tag: '{}', page: '{}', pageSize: '{}', inProgressOnly: '{}'", tagName, page, pageSize, inProgressOnly);
        var tag = await _entityRepository.QueryOneAsync(session => session.Query<Tag>().Where(x => x.Name == tagName));

        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>();
            if (tag != null)
            {
                query = query.Where(x => x.TagIds.Contains(tag.Id));
            }
            else
            {
                query = query.Where(x => x.TagIds.Count == 0);
            }

            if (inProgressOnly)
            {
                query = query.Where(x => x.Status == BookStatus.InProgress);
            }

            return query.OrderBy(x => x.Title)
                        .ThenByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select();
        });

        var total = await GetTagBookCount(tagName);
        return books.ToPagedList(page, pageSize, total);
    }

    [HttpGet("byCategory/{categoryName}/{page}/{pageSize}/{inProgressOnly}", Name = "getBooksByCategory")]
    public async Task<IPagedList<Book>> GetGroupedBooksByCategory(string categoryName, int page = 1, int pageSize = 30, bool inProgressOnly = false)
    {
        _logger.LogInformation("Get books by category: '{}', page: '{}', pageSize: '{}', inProgressOnly: '{}'", categoryName, page, pageSize, inProgressOnly);
        var category = await _entityRepository.QueryOneAsync(session => session.Query<Category>().Where(x => x.Name == categoryName));

        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>();
            if (category != null)
            {
                query = query.Where(x => x.CategoryIds.Contains(category.Id));
            }
            else
            {
                query = query.Where(x => x.CategoryIds.Count == 0);
            }

            if (inProgressOnly)
            {
                query = query.Where(x => x.Status == BookStatus.InProgress);
            }

            return query.OrderBy(x => x.Title)
                        .ThenByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select();
        });

        var total = await GetCategoryBookCount(categoryName);

        return books.ToPagedList(page, pageSize, total);
    }

    [HttpGet("byAuthor/{authorName}/{page}/{pageSize}/{inProgressOnly}", Name = "getBooksByAuthor")]
    public async Task<IPagedList<Book>> GetGroupedBooksByAuthor(string authorName, int page = 1, int pageSize = 30, bool inProgressOnly = false)
    {
        _logger.LogInformation("Get books by author: '{}', page: '{}', pageSize: '{}', inProgressOnly: '{}'", authorName, page, pageSize, inProgressOnly);
        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>().Where(x => x.Author == authorName);

            if (inProgressOnly)
            {
                query = query.Where(x => x.Status == BookStatus.InProgress);
            }

            return query.OrderBy(x => x.Title)
                        .ThenByDescending(x => x.DateUpdated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select();
        });

        var total = await GetAuthorbooksCount(authorName);
        return books.ToPagedList(page, pageSize, total);

    }

    [HttpGet("byStatus/{status}/{sortBy?}/{thenBy?}/{page}/{pageSize}/{keyword?}", Name = "getBooksByStatus")]
    public async Task<IPagedList<Book>> GetGroupedBooksByStatus(string? status, string? sortBy, string? thenBy,
        int page = 1, int pageSize = 12, string? keyword = null)
    {
        _logger.LogInformation("Get books by status: '{}', page: '{}', pageSize: '{}', keyword: '{}'", status, page, pageSize, keyword);
        Expression<Func<Book, bool>> filter = x => true;

        if (!string.IsNullOrWhiteSpace(status) && !status.Equals("all", StringComparison.CurrentCultureIgnoreCase))
        {
            filter = x => x.Status == status;
        }
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            sortBy = nameof(Book.DateUpdated);
        }
        if (string.IsNullOrWhiteSpace(thenBy))
        {
            thenBy = nameof(Book.Author);
        }

        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>().Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword, options: SearchOptions.Or)
                             .Search(x => x.Author, keyword, options: SearchOptions.Or);
            }


            return query
                        .OrderByPropertyOrField(sortBy.Trim(), true)
                        .ThenByPropertyOrField(thenBy.Trim())
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select();
        });

        var total = string.IsNullOrWhiteSpace(keyword) ? await GetStatusBookCount(status!) : await _entityRepository.CountAsync(session =>
        {
            var query = session.Query<Book>().Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword, options: SearchOptions.Or)
                             .Search(x => x.Author, keyword, options: SearchOptions.Or);
            }
            return query;
        });

        return books.ToPagedList(page, pageSize, total);
    }

    [HttpGet("publishQueue", Name = "getGroupedBooksInPublishQueue")]
    public async Task<IEnumerable<Book>> GetPublishQueue(int count = 36)
    {
        _logger.LogInformation("Get books in publish queue: '{}'", count);
        var books = await _entityRepository.QueryAsync(session => session.Query<Book>()
                                                                            .Where(x => x.Status == BookStatus.InProgress)
                                                                            .OrderBy(x => x.PublishOrder)
                                                                            .ThenByDescending(x => x.Author)
                                                                            .Take(count)
                                                                            .Select()
                                                                            );

        return books;
    }

    [HttpPost("search", Name = "search")]
    public async Task<IPagedList<Book>> SearchGroupedBooksByStatus([FromBody] BookSearchModel model)
    {
        _logger.LogInformation("Search book with status: '{}', keyword: '{}'", model.Status, model.Keyword);

        var sortBy = string.IsNullOrWhiteSpace(model.SortBy) ? nameof(Book.DateUpdated) : model.SortBy.Trim();
        var thenBy = string.IsNullOrWhiteSpace(model.ThenBy) ? nameof(Book.Author) : model.ThenBy.Trim();
        var keyword = model.Keyword;
        var noImage = model.NoImage ?? false;
        var status = model.Status ?? BookStatus.InProgress;

        _logger.LogInformation("Search book with status: '{}', keyword: '{}'", model.Status, model.Keyword);

        var books = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Book>().Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword)
                             .Search(x => x.Author, keyword);
            }

            if (noImage)
            {
                query = query.Where(x => x.ImageIds!.Count == 0);
            }


            var rawQuery = session.Advanced.AsyncRawQuery<Book>(query.ToString());
            _logger.LogInformation("Query: {}", rawQuery);

            return query
                        .OrderByPropertyOrField(sortBy, true)
                        .ThenByPropertyOrField(thenBy)
                        .Skip((model.Page - 1) * model.PageSize)
                        .Take(model.PageSize)
                        .Select();
        }
        );

        var total = string.IsNullOrWhiteSpace(keyword) ? await GetStatusBookCount(status!) : await _entityRepository.CountAsync(session =>
        {
            var query = session.Query<Book>().Where(x => x.Status == status);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword)
                             .Search(x => x.Author, keyword);
            }
            return query;
        }); ;

        return books.ToPagedList(model.Page, model.PageSize, total);
    }

    [HttpGet("workItems", Name = "workItems")]
    public async Task<IEnumerable<WorkProgress>> GetWorkItems()
    {
        return await _workProgressRepository.GetWorkingItemsAsync();
    }

    [HttpGet("categories", Name = "getCategoryNames")]
    public async Task<IEnumerable<string>> GetCategories()
    {
        _logger.LogInformation("Get all categories names");
        var rawCategories = await _entityRepository.QueryAsync(session => session.Query<Category>());
        var categories = rawCategories
                            .Select(x => string.IsNullOrEmpty(x.Name) ? "Unknown" : x.Name.Trim())
                            .Distinct();
        return categories;
    }

    [HttpGet("tags", Name = "getTagNames")]
    public async Task<IEnumerable<string>> GetTags()
    {
        _logger.LogInformation("Get all tags names");
        var rawTags = await _entityRepository.QueryAsync(session => session.Query<Tag>());

        var tags = rawTags.Select(x => string.IsNullOrWhiteSpace(x.Name) ? "Unknown" : x.Name.Trim())
                          .Distinct();
        return tags;
    }

    [HttpGet("speech/config", Name = "getSpeechConfig")]
    public Task<SpeechConfigModel> GetSpeechConfigAsync()
    {
        var model = _speechConfig.ToModel();

        return Task.FromResult(model);
    }

    [HttpPost("", Name = "create")]
    public async Task<Book> Create([FromBody] Book book)
    {
        _logger.LogInformation("Creating book {}", book.Title);

        book.Id = Guid.NewGuid().ToString();
        book.DateCreated = DateTime.Now;
        book.DateUpdated = DateTime.Now;
        book.Status = BookStatus.InProgress;
        book.TextCount = book.Content!.GetTextCount();
        book.PublishOrder = MaxOrder;

        await _entityRepository.UpdateAsync(book);

        return book;
    }

    [HttpPost("finish/{bookId}", Name = "finish")]
    public async Task<Book?> Finish(string bookId)
    {
        _logger.LogInformation("Finishing book with id: {}", bookId);

        var book = await _entityRepository.GetAsync<Book>(bookId);
        if (book != null)
        {
            book.Status = BookStatus.Finished;
            book.WavGenerated = true;
            book.Mp3Generated = true;
            book.Mp4Generated = true;
            book.SrtGenerated = true;
            book.Update();
            await _entityRepository.UpdateAsync(book);
        }

        return book;
    }

    [HttpPost("openBookFolder", Name = "openBookFolder")]
    public async Task OpenBookFolder([FromBody] OpenBookModel model)
    {
        _logger.LogInformation("Open book folder with id: {}", model.Id);

        var book = await _entityRepository.GetAsync<Book>(model.Id);
        if (book != null)
        {
            var folder = System.IO.Path.Combine(_bookConfig.BooksLocation, book.Author!, book.Title);
            folder.EnsureDirectoryExists();

            await _bookExportService.OpenBookFolder(folder);
        }
    }

    [HttpPut("updateRank/{id}/{rank}", Name = "updateRank")]
    public async Task<bool> UpdateRank(string id, float rank)
    {
        var book = await _entityRepository.GetAsync<Book>(id);
        if (book == null)
            return false;

        book.Rank = rank;
        book.PublishOrder = MaxOrder - rank;
        await _entityRepository.UpdateAsync(book);
        return true;
    }

    [HttpPut("updatePublishOrder/{id}/{order}", Name = "updatePublishOrder")]
    public async Task<Book?> UpdatePublishOrder(string id, float order)
    {
        var book = await _entityRepository.GetAsync<Book>(id);
        if (book == null)
            return null;

        book.PublishOrder = order;
        await _entityRepository.UpdateAsync(book);
        return book;
    }

    [HttpPut("updateCategoryTag", Name = "updateCategoryTag")]
    public async Task<bool> UpdateCategoryTag([FromBody] BookCategoryViewModel model)
    {
        var book = await _entityRepository.GetAsync<Book>(model.Id);
        if (book == null)
            return false;

        //book.Category = model.Category!;
        //book.Tag = model.Tag!;
        var result = await _entityRepository.UpdateAsync(book);
        return result;
    }

    [HttpPut("update", Name = "update")]
    public async Task<bool> Update([FromBody] Book book)
    {
        _logger.LogInformation("Updating book {}", book.Title);
        if (string.IsNullOrEmpty(book?.Content))
            return false;

        book.Update();

        await _entityRepository.UpdateAsync(book);
        return true;
    }

    [HttpPut("update/properties", Name = "updateProperties")]
    public async Task<bool> UpdateProperties([FromBody] Book book)
    {
        if (string.IsNullOrEmpty(book?.Content))
            return false;
        book.Update();
        await _entityRepository.UpdateAsync(book);
        return true;
    }

    [HttpPut("update/content", Name = "updateContent")]
    public async Task<bool> UpdateContent([FromBody] Book book)
    {
        if (string.IsNullOrEmpty(book?.Content))
            return false;

        book.Update();
        await _entityRepository.UpdateAsync(book);
        return true;
    }

    [HttpDelete("{id}", Name = "delete")]
    public async Task<bool> Delete(string id)
    {
        return await _entityRepository.DeleteAsync<Book>(id);
    }

    [HttpGet("files/{bookName}/{type}/{pattern?}", Name = "getBookFiles")]
    public IEnumerable<FileDescriptor> GetBookFiles(string bookName, string type, string pattern = "*")
    {
        var folder = System.IO.Path.Combine(_bookConfig.BooksLocation, bookName, type);
        var files = new List<FileDescriptor>();

        if (Directory.Exists(folder))
        {
            Directory
                .GetFiles(folder, pattern)
                .Select(x => new FileInfo(x))
                .OrderBy(x => x.Name, NaturalComparer.Instance)
                .ToList()
                .ForEach(fileInfo =>
                {
                    files.Add(new FileDescriptor()
                    {
                        Name = fileInfo.Name.GetFileNameWithoutExtension(),
                        Path = fileInfo.FullName,
                        Size = fileInfo.Length,
                        Duration = 0
                    });
                });
        }
        return files;
    }

    [HttpPost("export", Name = "exportBook")]
    public bool Export([FromBody] ExportModel model)
    {
        _bookExportService.ExportBook(model);

        return true;
    }

    [HttpPost("generateMeta/{bookId}", Name = "generateMeta")]
    public async Task<bool> GenerateMeta(string bookId)
    {
        var book = await _entityRepository.GetAsync<Book>(bookId);
        if (book != null)
        {
            await _metadataProcessor.GenerateBookMeta(book);
            return true;
        }

        return false;
    }

    [HttpGet("download/txt/{bookName}", Name = "downloadBookText")]
    public async Task<ActionResult> DownloadText(string bookName)
    {
        var book = await _entityRepository.GetByNameAsync(bookName);
        if (book == null)
        {
            return NotFound();
        }

        byte[] bytes = Encoding.UTF8.GetBytes(book.Content!);

        return File(bytes, "text/plain", $"{bookName}.txt");
    }

    [HttpPost("publish/library/{bookId}", Name = "publishBookToLibrary")]
    public async Task<ResultBase> PublishToLibrary(string bookId)
    {
        return await _librarySyncer.SyncBook(bookId);
    }

    [HttpPost("publish/library/finished", Name = "publishFinishedBooksToLibrary")]
    public async Task<IEnumerable<ResultBase>> PublishFinishedToLibrary()
    {
        var count = await _entityRepository.CountAsync(session => session.Query<Book>().Where(x => x.Status == BookStatus.Finished));
        var page = 1;
        var pageSize = 50;

        var results = new List<ResultBase>();

        while ((page - 1) * pageSize < count)
        {
            _logger.LogInformation("Processing page {page} of finished books", page);
            var books = await _entityRepository.QueryAsync(session =>
            {
                var query = session.Query<Book>()
                                   .Where(x => x.Status == BookStatus.Finished)
                                   .OrderByDescending(x => x.DateUpdated);
                return query.Skip((page - 1) * pageSize)
                            .Take(pageSize);

            });
            page++;

            var result = await _librarySyncer.SyncBooks(books.Select(x => x.Id));
            results.AddRange(result);
        }

        return results;
    }

    [HttpPost("reset/{bookId}", Name = "resetBookStatus")]
    public async Task<ResultBase> ResetBookStatus(string bookId)
    {
        var book = await _entityRepository.GetByIdAsync<Book>(bookId);
        book.WavGenerated = false;
        book.Mp3Generated = false;
        book.Mp4Generated = false;
        book.SrtGenerated = false;
        book.TextGenerated = false;
        book.DateUpdated = DateTime.Now;
        await _entityRepository.UpdateAsync(book);

        return ResultBase.OK();
    }

    [HttpPost("initFolders/{bookId}", Name = "initFolders")]
    public async Task<ResultBase> InitFolders(string bookId)
    {
        var book = await _entityRepository.GetByIdAsync<Book>(bookId);
        if (book == null)
        {
            return ResultBase.Fail("Book not found");
        }

        var artistFolder = IO.Path.Combine(_bookConfig.BooksLocation, book.Author!);
        artistFolder.EnsureDirectoryExists();
        var bookFolder = IO.Path.Combine(artistFolder, book.Title);
        bookFolder.EnsureDirectoryExists();

        IO.Path.Combine(bookFolder, "wav").EnsureDirectoryExists();
        IO.Path.Combine(bookFolder, "mp3").EnsureDirectoryExists();
        IO.Path.Combine(bookFolder, "mp4").EnsureDirectoryExists();
        IO.Path.Combine(bookFolder, "srt").EnsureDirectoryExists();
        IO.Path.Combine(bookFolder, "txt").EnsureDirectoryExists();
        IO.Path.Combine(bookFolder, "images").EnsureDirectoryExists();

        await _bookExportService.OpenBookFolder(bookFolder);

        return ResultBase.OK();
    }

    [HttpGet("generateSubtitleJobSchedule/{count}", Name = "generateSubtitleJobSchedule")]
    public async Task<ResultBase> GenerateSubtitleJobSchedule(int count = 100)
    {
        var books = await _entityRepository.QueryAsync(session => session.Query<Book>()
                                                                         .Where(x => x.Status == BookStatus.Finished)
                                                                         .OrderByDescending(x => x.DateUpdated)
                                                                         .Take(count));
        var lines = new StringBuilder();
        foreach (var book in books)
        {
            var srtFolder = IO.Path.Combine(_bookConfig.BooksLocation, book.Author!, book.Title.Trim(), ResourceTypes.Srt);
            var mp3Folder = IO.Path.Combine(_bookConfig.BooksLocation, book.Author!, book.Title.Trim(), ResourceTypes.Mp3);

            if (Directory.Exists(srtFolder))
            {
                var mp3Files = Directory.GetFiles(mp3Folder, $"*.{ResourceTypes.Mp3}");
                var files = Directory.GetFiles(srtFolder, $"*.{ResourceTypes.Srt}");
                if (files.Length == mp3Files.Length)
                {
                    continue;
                }
            }

            srtFolder.EnsureDirectoryExists();

            lines.AppendLine($"{mp3Folder}, {srtFolder}");
        }
        return ResultBase.OK(lines.ToString());
    }

    [HttpGet("{bookId}/chapters", Name = "getChapterTitles")]
    public async Task<ResultBase> GetChapterTitles(string bookId)
    {
        var book = await _entityRepository.GetByIdAsync<Book>(bookId);
        if (book == null)
        {
            return ResultBase.Fail("Book not found");
        }
        var chapters = book.GetChapterNames();
        if (chapters == null || !chapters.Any())
        {
            return ResultBase.Fail("No chapters found in the book content.");
        }

        return ResultBase.OK(chapters);
    }

    [HttpPost("enhance/mp3/{bookId}", Name = "enhanceBookMp3Audio")]
    public async Task<ResultBase> EnhanceMp3(string bookId)
    {
        var book = await _entityRepository.GetByIdAsync<Book>(bookId);
        if (book == null)
        {
            return ResultBase.Fail("Book not found");
        }

        await _bookExportService.EnhanceMp3(book);

        return ResultBase.OK();
    }

    private void LogQuery<T>(IAsyncDocumentSession session, IRavenQueryable<T> query)
    {
        var rawQuery = session.Advanced.AsyncRawQuery<T>(query.ToString());
        _logger.LogInformation("Query: {}", rawQuery);
    }

    private async Task<int> GetCategoryBookCount(string categoryName)
    {
        var category = await _entityRepository.QueryOneAsync(session => session.Query<BookGroupModel, BooksByCategoryAndCount>().Where(x => x.Name == categoryName));

        return category.Count;
    }

    private async Task<int> GetTagBookCount(string tagName)
    {
        var tag = await _entityRepository.QueryOneAsync(session => session.Query<BookGroupModel, BooksByTagAndCount>().Where(x => x.Name == tagName));
        return tag.Count;
    }

    private async Task<int> GetStatusBookCount(string status)
    {
        var tag = await _entityRepository.QueryOneAsync(session => session.Query<BookGroupModel, BooksByStatusAndCount>().Where(x => x.Name == status));
        return tag.Count;
    }

    private async Task<int> GetAuthorbooksCount(string authorName)
    {
        var books = await _entityRepository.QueryAsync(session => session.Query<Book>().Where(x => x.Author == authorName));
        return books.Count();
    }
}
