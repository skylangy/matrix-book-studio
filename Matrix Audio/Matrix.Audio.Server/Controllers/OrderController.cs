using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.Common;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raven.Client.Documents;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class OrderController(IEntityRepository entityRepository,
    IOrderProcessor orderProcessor) : ControllerBase
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IOrderProcessor _orderProcessor = orderProcessor;

    [HttpGet("search/{keyword}/{page}/{pageSize}", Name = "searchOrders")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> Search(string keyword, int page = 1, int pageSize = 10)
    {
        var orders = await _entityRepository.QueryAsync(session =>
        {
            var query = session.Query<Order>();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Search(x => x.OrderNumber, keyword);
            }

            return query.OrderByDescending(x => x.DateCreated)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize);
        });
        return Ok(orders);
    }

    [HttpGet("{page}/{pageSize}", Name = "getOrders")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetOrders(int page = 1, int pageSize = 10)
    {
        var orders = await _entityRepository.QueryAsync(session => session.Query<Order>()
                                                                          .OrderByDescending(x => x.DateCreated)
                                                                          .Skip((page - 1) * pageSize)
                                                                          .Take(pageSize)
                                                                         );
        return Ok(orders);
    }

    [HttpGet("summary/for/user/{userId}", Name = "getOrdersForUser")]
    [Cache]
    [ResponseCache(Duration = CacheSettings.CacheDurationInMinuteShort, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetOrdersForUser(string userId)
    {
        var orders = await _entityRepository.QueryAsync(session => session.Query<Order>()
                                                                          .Where(x => x.UserId == userId && x.OrderStatus != OrderStatuses.Removed)
                                                                          .OrderByDescending(x => x.DateCreated));

        var orderViewModels = orders.Select(x => new OrderViewModel
        {
            OrderId = x.Id,
            UserId = x.UserId,
            OrderNumber = x.OrderNumber,
            OrderStatus = x.OrderStatus,
            ProductName = x.Items.FirstOrDefault()?.ProductName ?? string.Empty,
            Subtotal = x.Subtotal,
            Total = x.Total,
            DateCreated = x.DateCreated
        });
        return Ok(orderViewModels);
    }

    [HttpGet("{id}", Name = "getOrder")]
    public async Task<IActionResult> GetOrderSummary(string id)
    {
        var order = await _entityRepository.GetAsync<Order>(id);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    [HttpPost(Name = "createOrder")]
    [Authorize]
    public async Task<IActionResult> CreateOrder([FromBody] Order order)
    {
        if (order == null)
        {
            return BadRequest(new ResultBase { Success = false, Message = "Invalid order" });
        }

        await _entityRepository.UpdateAsync(order);
        var result = await _orderProcessor.Process(order);
        return Ok(new ResultBase { Success = true, Message = result.Message });
    }

    [HttpPost("update/status/{id}/{status}", Name = "updateStatus")]
    [Authorize]
    public async Task<IActionResult> UpdateOrderStatus(string id, string status)
    {
        var order = await _entityRepository.GetAsync<Order>(id);
        order.OrderStatus = status;
        await _entityRepository.UpdateAsync(order);
        return Ok(new ResultBase { Success = true });
    }
}
