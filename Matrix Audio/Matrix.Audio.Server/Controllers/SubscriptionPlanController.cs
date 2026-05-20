using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents.Linq;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SubscriptionPlanController(
    IEntityRepository entityRepository,
    IConfiguration configuration) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IConfiguration _configuration = configuration;


    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    [HttpGet("{page}/{pageSize}", Name = "getSubscriptionPlans")]
    public async Task<IActionResult> GetSubscriptionPlans(int page = 1, int pageSize = 10)
    {
        var plans = await _entityRepository.QueryAsync(session => session.Query<SubscriptionPlan>()
                                                                          .OrderByDescending(x => x.DateCreated)
                                                                          .Skip((page - 1) * pageSize)
                                                                          .Take(pageSize));
        return Ok(plans);
    }

    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    [HttpGet("assignable", Name = "getAssignableSubscriptionPlans")]
    public async Task<IActionResult> GetAssignableSubscriptionPlans()
    {
        var setting = await _entityRepository.GetAsync<AppSetting>(AppSettingKeys.AppAssignableSubIds);
        var subIds = setting.Value.Split(',').Select(x => x.Trim());


        var plans = await _entityRepository.QueryAsync(session => session.Query<SubscriptionPlan>()
                                                                    .Where(x => x.Id.In(subIds))
                                                                    );
        return Ok(plans);
    }

    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    [HttpGet("{id}", Name = "getSubscriptionPlan")]
    public async Task<IActionResult> GetSubscriptionPlan(string id)
    {
        var plan = await _entityRepository.GetAsync<SubscriptionPlan>(id);
        if (plan == null)
        {
            return NotFound();
        }
        return Ok(plan);
    }

    [HttpPost(Name = "createSubscriptionPlan")]
    [Authorize]
    public async Task<IActionResult> CreateSubscriptionPlan([FromBody] SubscriptionPlan subscriptionPlan)
    {
        if (subscriptionPlan == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "Invalid subscription plan" });
        }
        await _entityRepository.UpdateAsync(subscriptionPlan);

        return Ok(new ResultBase { Success = true });
    }

    [HttpPut(Name = "updateSubscriptionPlan")]
    [Authorize]
    public async Task<IActionResult> UpdateSubscriptionPlan([FromBody] SubscriptionPlan subscriptionPlan)
    {
        if (subscriptionPlan == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "Invalid subscription plan" });
        }
        await _entityRepository.UpdateAsync(subscriptionPlan);
        return Ok(new ResultBase { Success = true });
    }

    [HttpDelete("{id}", Name = "deleteSubscriptionPlan")]
    [Authorize]
    public async Task<IActionResult> DeleteSubscriptionPlan(string id)
    {
        await _entityRepository.DeleteAsync<SubscriptionPlan>(id);
        return Ok(new ResultBase { Success = true });
    }

    [HttpPost("populate/from/config", Name = "initSubscriptionPlans")]
    //[Authorize]
    public async Task<IActionResult> InitSubscriptionPlans()
    {
        await _entityRepository.PopulatePlanFromConfig(_configuration);
        return Ok(new ResultBase { Success = true });
    }

    [HttpGet("from/config", Name = "getPlansFromConfig")]
    //[Authorize]
    public async Task<IActionResult> GetPlansFromConfig()
    {
        var plans = await _entityRepository.GetPlansFromConfig(_configuration);
        return Ok(plans);
    }
}
