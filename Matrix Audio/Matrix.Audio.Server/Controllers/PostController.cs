using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PostController(
    IEntityRepository entityRepository,
    ILogger<PostController> logger
    ) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly ILogger<PostController> _logger = logger;

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchPosts")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var posts = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Post>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.Title, keyword)
                             .Search(x => x.Content, keyword);
            }

            return query.OrderByDescending(x => x.DateCreated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        return Ok(posts);
    }

    [HttpGet("all/{page}/{pageSize}", Name = "getAllPosts")]
    [Cache]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 12)
    {
        var posts = await _entityRepository.QueryAsync(session => session.Query<Post>()
                                                                         .OrderByDescending(x => x.DateCreated)
                                                                         .Skip((page - 1) * pageSize)
                                                                         .Take(pageSize));

        return Ok(posts);
    }

    [HttpGet("for/edit/{id}", Name = "getPostForEdit")]
    [Cache]
    public async Task<IActionResult> GetPostForEdit(string id)
    {
        var post = await _entityRepository.GetAsync<Post>(id);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    [HttpGet("recents/{count}", Name = "recentPosts")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetRecents(int count = 5)
    {
        var posts = await _entityRepository.QueryAsync(session => session.Query<Post>().OrderByDescending(x => x.DateCreated).Take(count));

        return Ok(posts);
    }

    [HttpGet("{id}", Name = "getPost")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.ResponseCacheDurationLong, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Get(string id)
    {
        var post = await _entityRepository.GetAsync<Post>(id);
        if (post == null)
        {
            return NotFound();
        }
        return Ok(post);
    }

    [HttpPost("create", Name = "createPost")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] Post post)
    {
        post.Id = Guid.NewGuid().ToString();
        post.DateCreated = DateTime.Now;
        post.DateUpdated = DateTime.Now;
        post.Slug = post.Title?.ToSlug();

        _logger.LogInformation("Creating post with slug: {}", post.Slug);

        while (await _entityRepository.IsSlugUsed(post.Slug!))
        {
            post.Slug = post.Title!.ToSlug(post.Title!.GenerateRandomString());
        }

        await _entityRepository.UpdateAsync(post);
        return Ok(post);
    }

    [HttpPost("remove/splash/{id}", Name = "removePostSplash")]
    [Authorize]
    public async Task<IActionResult> RemoveSplash(string id)
    {
        var post = await _entityRepository.GetAsync<Post>(id);
        post.Image = null;
        await _entityRepository.UpdateAsync(post);
        return Ok(post);
    }

    [HttpPut("update", Name = "updatePost")]
    [Authorize]
    public async Task<IActionResult> Update([FromBody] Post post)
    {
        post.DateUpdated = DateTime.Now;
        var result = await _entityRepository.UpdateAsync(post);
        if (!result)
        {
            return BadRequest();
        }
        return Ok();
    }

    [HttpDelete("{id}", Name = "deletePost")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _entityRepository.DeleteAsync<Post>(id);
        if (!result)
        {
            return NotFound();
        }
        return Ok();
    }
}
