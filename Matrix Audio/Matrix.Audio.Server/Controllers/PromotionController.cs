using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matrix.Audio.Server.Controllers;


[ApiController]
[Route("api/v1/[controller]")]
public class PromotionController(IEntityRepository entityRepository) : ControllerBase
{
    private const string CacheKey = "Promotion";
    private readonly IEntityRepository _entityRepository = entityRepository;

    [HttpGet("all/{page}/{pageSize}", Name = "getPromotions")]
    [Cache(KeyPrefix = CacheKey)]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetPromotions(int page = 1, int pageSize = 10)
    {
        var promos = await _entityRepository.QueryAsync(session => session.Query<Promo>()
                                                                          .OrderByDescending(x => x.DateCreated)
                                                                          .Skip((page - 1) * pageSize)
                                                                          .Take(pageSize));
        return Ok(promos);
    }

    [HttpGet("{id}", Name = "getPromo")]
    [Cache(KeyPrefix = CacheKey)]
    public async Task<IActionResult> GetPromo(string id)
    {
        var promo = await _entityRepository.GetAsync<Promo>(id);
        if (promo == null)
        {
            return NotFound();
        }
        return Ok(promo);
    }

    [HttpGet("promos", Name = "getPromos")]
    [Cache(KeyPrefix = CacheKey)]
    public async Task<IActionResult> GetPromos()
    {
        var promos = await _entityRepository.GetAllAsync<Promo>();
        return Ok(promos);
    }

    [HttpGet("bycode/{code}", Name = "getPromoByCode")]
    [Cache(KeyPrefix = CacheKey)]
    public async Task<IActionResult> GetPromoByCode(string code)
    {
        var promo = await _entityRepository.QueryOneAsync(session => session.Query<Promo>().Where(x => x.Code == code));
        if (promo == null)
        {
            return NotFound();
        }
        return Ok(promo);
    }

    [HttpPost("promo", Name = "createPromo")]
    [Authorize]
    [ResetCache(CacheKey)]
    public async Task<IActionResult> CreatePromo([FromBody] Promo promo)
    {
        if (promo == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "Invalid promo" });
        }
        promo.ValidFrom = promo.ValidFrom.ToUniversalTime();
        promo.ValidTo = promo.ValidTo.ToUniversalTime();
        await _entityRepository.UpdateAsync(promo);
        return Ok(new ResultBase { Success = true });
    }

    [HttpPost(Name = "updatePromo")]
    [Authorize]
    [ResetCache(CacheKey)]
    public async Task<IActionResult> UpdatePromo([FromBody] Promo promo)
    {
        if (promo == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "Invalid promo" });
        }

        promo.ValidFrom = promo.ValidFrom.ToUniversalTime();
        promo.ValidTo = promo.ValidTo.ToUniversalTime();

        await _entityRepository.UpdateAsync(promo);
        return Ok(new ResultBase { Success = true });
    }

    [HttpDelete("{id}", Name = "deletePromo")]
    [Authorize]
    [ResetCache(CacheKey)]
    public async Task<IActionResult> Delete(string id)
    {
        await _entityRepository.DeleteAsync<Promo>(id);
        return Ok(new ResultBase { Success = true });
    }
}
