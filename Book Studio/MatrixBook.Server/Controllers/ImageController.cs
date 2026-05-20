using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Models.Data;
using AudioBookStudio.Models.Extensions;
using MatrixBook.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using IO = System.IO;

namespace MatrixBook.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ImageController(
    IOptions<AppConfiguration> bookConfig,
    IEntityRepository entityRepository,
    IImageService imageService,
    IBookImageCache bookImageCache,
    ILogger<ImageController> logger
    ) : ControllerBase
{
    private readonly AppConfiguration _bookConfig = bookConfig.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IBookImageCache _bookImageCache = bookImageCache;
    private readonly IImageService _imageService = imageService;
    private readonly ILogger<ImageController> _logger = logger;

    [HttpPost("", Name = "uploadBookImage")]
    public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string bookName)
    {
        if (file == null || file.Length == 0 || !IsSupportedImage(file.FileName))
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var book = await _entityRepository.GetByNameAsync(bookName);
            if (book == null)
            {
                return NotFound($"Book '{bookName}' is not found");
            }
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _bookConfig.BooksLocation, book.Author!, bookName, "images");
            if (string.IsNullOrEmpty(uploadsFolder))
            {
                return BadRequest("Invalid book name or author name.");
            }

            var filePath = Path.Combine(uploadsFolder, file.FileName);
            uploadsFolder.EnsureDirectoryExists();

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            book.ImageIds ??= [];

            var (imgWidth, imgHeight) = _imageService.GetImageSize(filePath);
            var imageResource = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName.GetFileNameWithoutExtension(),
                FolderName = bookName,
                Width = imgWidth,
                Height = imgHeight,
            };
            await _entityRepository.UpdateAsync(imageResource);

            book.ImageIds.Add(imageResource.Id);
            book.DefaultImageId = imageResource.Id;
            await _entityRepository.UpdateAsync(book);

            return Ok(book);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("author", Name = "uploadAuthorImage")]
    public async Task<IActionResult> UploadAuthorImage(IFormFile file, [FromForm] string authorName)
    {
        if (file == null || file.Length == 0 || !IsSupportedImage(file.FileName))
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var author = await _entityRepository.GetAuthorByNameAsync(authorName);
            if (author == null)
            {
                return NotFound($"Author '{authorName}' is not found.");
            }
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), _bookConfig.BooksLocation, authorName, "images");
            if (string.IsNullOrEmpty(uploadsFolder))
            {
                return BadRequest("Invalid author name.");
            }

            uploadsFolder.EnsureDirectoryExists();

            var filePath = GetUniqueFileName(uploadsFolder, $"{authorName}{Path.GetExtension(file.FileName)}");

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var (imageWidth, imageHeight) = _imageService.GetImageSize(filePath);
            var image = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = file.FileName.GetFileNameWithoutExtension(),
                FolderName = authorName,
                Width = imageWidth,
                Height = imageHeight,

            };
            author.ImageIds.Add(image.Id);
            author.Image = image.Id;

            await _entityRepository.UpdateAsync(author);

            return Ok(author);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{bookName}/{fileName}", Name = "getImage")]
    public async Task<IActionResult> Get(string bookName, string fileName)
    {
        var key = $"Book_{bookName}_{fileName}";
        var (exists, imagePath) = _bookImageCache.GetImagePath(key);
        if (exists)
        {
            return PhysicalFile(imagePath, "image/jpg");
        }

        var book = await _entityRepository.GetByNameAsync(bookName);
        if (book == null)
        {
            return NotFound();
        }

        var etag = $"\"{key.GetHashCode()}\"";
        if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
        {
            return StatusCode(304); // Not Modified
        }

        var imageResource = await _entityRepository.GetByIdAsync<ImageResource>(fileName);
        if (imageResource == null)
        {
            return NotFound();
        }

        var fullImagePath = GetImageFilePath(bookName, book.Author!, imageResource.FileName!);
        if (IO.File.Exists(fullImagePath))
        {
            _bookImageCache.SetImagePath(key, fullImagePath);

            var extension = Path.GetExtension(fullImagePath);
            var clientFilename = $"{key}{extension}";
            Response.Headers.CacheControl = "public, max-age=31536000";             // Cache for 1 year
            Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");    // Optional
            Response.Headers.ETag = $"\"{key.GetHashCode()}\"";                      // Optional: Use an ETag for validation

            return PhysicalFile(fullImagePath, "image/jpg", clientFilename);
        }

        return NotFound();
    }

    [HttpGet("author/{authorName}/{fileName}", Name = "getAuthorImage")]
    public async Task<IActionResult> GetAuthorImage(string authorName, string fileName)
    {
        if (string.IsNullOrWhiteSpace(authorName) || string.IsNullOrWhiteSpace(fileName))
        {
            return NotFound();
        }

        var key = $"Author_{authorName}_{fileName}";
        var (exists, imagePath) = _bookImageCache.GetImagePath(key);
        if (exists)
        {
            return PhysicalFile(imagePath, "image/jpg");
        }

        var imageResource = await _entityRepository.GetByIdAsync<ImageResource>(fileName);
        if (imageResource == null)
        {
            return NotFound();
        }

        var fullImagePath = Path.Combine(_bookConfig.BooksLocation, authorName, "images", imageResource.FileName!);
        if (IO.File.Exists(fullImagePath))
        {
            _bookImageCache.SetImagePath(key, fullImagePath);
            return PhysicalFile(fullImagePath, "image/jpg");
        }

        return NotFound();
    }

    [HttpDelete("{bookName}/{fileId}", Name = "deleteImage")]
    public async Task<IActionResult> Delete(string bookName, string fileId)
    {
        var book = await _entityRepository.GetByNameAsync(bookName);
        if (book == null)
        {
            return NotFound($"Book '{bookName}' is not found");
        }

        _logger.LogInformation("Deleting image '{fileName}' from book '{bookName}'", fileId, bookName);

        var imageResource = await _entityRepository.GetByIdAsync<ImageResource>(fileId);

        if (imageResource == null)
        {
            return NotFound($"Image '{fileId}' is not found");
        }

        book.ImageIds ??= [];
        book.ImageIds.Remove(fileId);
        await _entityRepository.UpdateAsync(book);

        _logger.LogInformation("Image '{fileName}' is deleted from database, now remove the physical file", fileId);

        var full = GetImageFilePath(bookName, book.Author!, imageResource.FileName!);
        if (IO.File.Exists(full))
        {
            IO.File.Delete(full);
            _logger.LogInformation("Physical file '{full}' is deleted", full);
        }

        return Ok();
    }

    [HttpGet("by/id/{imageId}", Name = "getImagePathById")]
    public async Task<IActionResult> GetImagePathById(string imageId)
    {
        var (exists, imagePath) = _bookImageCache.GetImagePath(imageId);
        if (exists)
        {
            return PhysicalFile(imagePath, "image/jpg");
        }

        var etag = $"\"{imageId.GetHashCode()}\"";
        if (Request.Headers.TryGetValue("If-None-Match", out var clientEtag) && clientEtag == etag)
        {
            return StatusCode(304); // Not Modified
        }

        var image = await _entityRepository.GetByIdAsync<ImageResource>(imageId);

        if (image != null)
        {
            var fullImagePath = Path.Combine(_bookConfig.BooksLocation, image.FolderName!, "images", image.FileName!);
            if (IO.File.Exists(fullImagePath))
            {
                _bookImageCache.SetImagePath(imageId, fullImagePath);

                var extension = Path.GetExtension(fullImagePath);
                var clientFilename = $"{imageId}{extension}";
                Response.Headers.CacheControl = "public, max-age=31536000";                  // Cache for 1 year
                Response.Headers.Expires = DateTime.UtcNow.AddYears(1).ToString("R");        // Optional
                Response.Headers.ETag = $"\"{imageId.GetHashCode()}\"";                      // Optional: Use an ETag for validation


                return PhysicalFile(fullImagePath, "image/jpg", clientFilename);
            }
        }

        return NotFound();
    }

    [HttpGet("resource/by/id/{imageId}", Name = "getImageResourceById")]
    public async Task<IActionResult> GetImageResourceById(string imageId)
    {
        var image = await _entityRepository.GetByIdAsync<ImageResource>(imageId);
        return Ok(image);
    }

    [HttpPost("fix/splashes/all", Name = "fixSplashes")]
    public async Task<IActionResult> FixSplashes()
    {
        var books = await _entityRepository.GetAllAsync<Book>();
        int updated = await CheckBookSplash(books);
        var result = new OperationResult { Success = true, Message = $"{updated} books were updated" };
        return Ok(result);
    }

    [HttpPost("fix/splashes/missing", Name = "fixSplashesMissing")]
    public async Task<IActionResult> FixSplashesMissing()
    {
        var books = await _entityRepository.QueryAsync(session => session.Query<Book>()
                                                                         .Where(x => x.DefaultImageId == null));

        int updated = await CheckBookSplash(books);
        var result = new OperationResult { Success = true, Message = $"{updated} books were updated" };
        return Ok(result);
    }

    [HttpPost("fix/splashes/book/{bookId}", Name = "fixBookSplash")]
    public async Task<IActionResult> FixBookSplash(string bookId)
    {
        var book = await _entityRepository.GetByIdAsync<Book>(bookId);
        if (book == null)
        {
            return NotFound($"Book '{bookId}' is not found");
        }

        int updated = await CheckBookSplash(book);
        var result = new OperationResult { Success = true, Message = $"{updated} books were updated" };
        return Ok(result);
    }

    [HttpPost("fix/splashes/author", Name = "fixAuthorSplashes")]
    public async Task<IActionResult> FixAuthorSplashes()
    {
        var authors = await _entityRepository.GetAllAsync<Author>();

        OperationResult result = await CheckAuthorAvatar(authors);
        return Ok(result);
    }

    [HttpPost("fix/splashes/author/missing", Name = "fixAuthorSplashesMissing")]
    public async Task<IActionResult> FixAuthorSplashesMissing()
    {
        var authors = (await _entityRepository.QueryAsync(session => session.Query<Author>())).ToList();

        await RemoveDuplicates(authors);

        var authorsFromFolder = IO.Directory.GetDirectories(_bookConfig.BooksLocation)
                                  .Select(x => IO.Path.GetFileName(x).Trim())
                                  .ToList();

        var missingAuthors = authorsFromFolder.Except(authors.Select(x => x.Name)).ToList();
        foreach (var authorName in missingAuthors)
        {
            var author = new Author
            {
                Id = Guid.NewGuid().ToString(),
                Name = authorName,
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            };
            await _entityRepository.UpdateAsync(author);
            authors.Add(author);
        }

        OperationResult result = await CheckAuthorAvatar(authors);
        return Ok(result);
    }

    [HttpPost("convert/png/to/jpg", Name = "convertPngToJpb")]
    public async Task ConvertPngToJpb()
    {
        var images = await _entityRepository.GetAllAsync<ImageResource>();
        foreach (var image in images)
        {
            if (image.FileName?.EndsWith(ResourceTypes.SplashType, StringComparison.OrdinalIgnoreCase) == true)
            {
                image.FileName = $"{Path.GetFileNameWithoutExtension(image.FileName)}.jpg";
                await _entityRepository.UpdateAsync(image);
            }
        }
    }

    private string GetImageFilePath(string bookName, string authorName, string fileName)
    {
        if (string.IsNullOrWhiteSpace(bookName) || string.IsNullOrWhiteSpace(authorName) || string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }
        var full = Path.Combine(_bookConfig.BooksLocation, authorName, bookName, "images", fileName);
        return full;
    }

    private static bool IsSupportedImage(string fileName)
    {
        var supportedExtensions = new HashSet<string>() { ".jpg", ".jpeg", ".png" };
        return !string.IsNullOrEmpty(fileName)
            && supportedExtensions.Contains(Path.GetExtension(fileName).ToLower());
    }

    private static string GetUniqueFileName(string folder, string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);

        var fullName = Path.Combine(folder, fileName);
        while (IO.File.Exists(fullName))
        {
            fileName = $"{name}_{Guid.NewGuid().ToString()[..4]}{ext}";
            fullName = Path.Combine(folder, fileName);
        }

        return fullName;
    }

    private async Task<int> CheckBookSplash(IEnumerable<Book> books)
    {
        int updated = 0;
        foreach (var book in books)
        {
            updated += await CheckBookSplash(book);
        }

        return updated;
    }

    private async Task<int> CheckBookSplash(Book book)
    {
        int updated = 0;
        var imagesFolder = Path.Combine(_bookConfig.BooksLocation, book.Author!, book.Title!, "images");
        imagesFolder.EnsureDirectoryExists();

        var folderName = @$"{book.Author}\{book.Title}";
        var splashName = $"{book.Title}-wide-splash.jpg";
        var imageResource = await _entityRepository.QueryOneAsync(session => session.Query<ImageResource>()
                                                                              .Where(x => x.FolderName == folderName && x.FileName == splashName));

        var fullImageFile = GetImageFilePath(book.Title!, book.Author!, splashName);
        if (imageResource == null && IO.File.Exists(fullImageFile))
        {
            var (Width, Height) = _imageService.GetImageSize(fullImageFile);
            imageResource = new ImageResource
            {
                Id = Guid.NewGuid().ToString(),
                FileName = splashName,
                FolderName = folderName,
                Width = Width,
                Height = Height,
            };
            await _entityRepository.UpdateAsync(imageResource);
        }

        if (imageResource == null)
        {
            return updated;
        }

        book.DefaultImageId = imageResource!.Id;
        book.ImageIds.Clear();
        await _entityRepository.UpdateAsync(book);

        updated++;

        _logger.LogInformation("Book '{}' is updated", book.Title);

        return updated;
    }

    private async Task RemoveDuplicates(List<Author> authors)
    {
        var duplicateAuthors = authors.GroupBy(x => x.Name)
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key)
                                    .ToList();

        foreach (var authorName in duplicateAuthors)
        {
            var duplicate = authors.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name) && x.Name.Equals(authorName, StringComparison.InvariantCultureIgnoreCase) && string.IsNullOrEmpty(x.Description));

            if (duplicate != null)
            {
                await _entityRepository.DeleteAsync<Author>(duplicate.Id);
                authors.Remove(duplicate);
                _logger.LogInformation("Duplicate author '{}' is removed", authorName);
            }
        }
    }

    private async Task<OperationResult> CheckAuthorAvatar(IEnumerable<Author> authors)
    {
        int updated = 0;
        foreach (var author in authors)
        {
            var imagesFolder = Path.Combine(_bookConfig.BooksLocation, author.Name!, "images");
            imagesFolder.EnsureDirectoryExists();

            var folderName = @$"{author.Name}";
            var splashName = $"{author.Name}{ResourceTypes.SplashType}";
            var imageResource = await _entityRepository.QueryOneAsync(session => session.Query<ImageResource>()
                                                                                        .Where(x => x.FolderName == folderName && x.FileName == splashName));
            var fullImageFile = Path.Combine(imagesFolder, splashName);
            if (!IO.File.Exists(fullImageFile))
            {
                var sourceAvatar = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "assets", "images", "avatars", "default-avatar.jpg");
                IO.File.Copy(sourceAvatar, fullImageFile, true);
            }

            if (imageResource == null && IO.File.Exists(fullImageFile))
            {
                var (Width, Height) = _imageService.GetImageSize(fullImageFile);
                var image = new ImageResource
                {
                    Id = Guid.NewGuid().ToString(),
                    FileName = splashName,
                    FolderName = folderName,
                    Width = Width,
                    Height = Height,
                };
                await _entityRepository.UpdateAsync(image);
                author.Image = image.Id;
                await _entityRepository.UpdateAsync(author);
                updated++;
                _logger.LogInformation("Artist '{}' is updated", author.Name);
            }
        }
        var result = new OperationResult { Success = true, Message = $"{updated} artists were updated" };
        return result;
    }
}
