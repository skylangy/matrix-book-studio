using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;

namespace Matrix.Audio.Common.Services;
public class OrderProcessor(
    IEntityRepository entityRepository,
    IEmailService emailService,
    ILogger<OrderProcessor> logger) : IOrderProcessor
{
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly ILogger<OrderProcessor> _logger = logger;

    public async Task<OrderProcessResult> Process(Order order)
    {
        try
        {
            _logger.LogInformation("Starting order processing for Order ID: {OrderId}", order.Id);

            await CreateSubscription(order);

            await SendConfirmationEmail(order);

            _logger.LogInformation("Order processing completed successfully for Order ID: {OrderId}", order.Id);

            var updateOrder = await _entityRepository.GetAsync<Order>(order.Id);
            updateOrder.OrderStatus = OrderStatuses.Completed;
            await _entityRepository.UpdateAsync(updateOrder);

            return new OrderProcessResult
            {
                OrderId = order.Id,
                IsSuccess = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing Order ID: {OrderId}", order.Id);
            return new OrderProcessResult
            {
                OrderId = order.Id,
                IsSuccess = false,
                Message = "An unexpected error occurred. Please try again later."
            };
        }
    }

    private async Task CreateSubscription(Order order)
    {
        _logger.LogInformation("Creating subscription for user {UserId}", order.UserId);
        var orderItem = order.Items.First();

        var userSubscription = await _entityRepository.QueryOneAsync(session => session.Query<UserSubscription>()
                                                                                   .Where(x => x.UserId == order.UserId));
        var user = await _entityRepository.GetAsync<User>(order.UserId);
        var subscription = await _entityRepository.GetAsync<SubscriptionPlan>(orderItem.ProductId);
        if (user != null && subscription != null)
        {
            user.Level = subscription.Level;
            await _entityRepository.UpdateAsync(user);
        }

        var now = DateTime.UtcNow;
        if (userSubscription == null)
        {
            userSubscription = new UserSubscription
            {
                Id = Guid.NewGuid().ToString(),
                UserId = order.UserId,
                SubscriptionId = orderItem.ProductId,
                DateCreated = now,
                StartDate = now,
                EndDate = now.AddDays(orderItem.Quantity),
                IsActive = true,
                Name = orderItem.ProductName,
                PeriodInDays = orderItem.Quantity
            };
        }
        else
        {
            userSubscription.SubscriptionId = orderItem.ProductId;
            userSubscription.StartDate = now;
            userSubscription.EndDate = now.AddDays(orderItem.Quantity);
            userSubscription.PeriodInDays = orderItem.Quantity;
            userSubscription.Name = orderItem.ProductName;
        }

        await _entityRepository.UpdateAsync(userSubscription);

        _logger.LogInformation("Subscription created for User ID: {UserId}, Plan ID: {PlanId}", order.UserId, order.OrderNumber);
    }

    private async Task SendConfirmationEmail(Order order)
    {
        _logger.LogInformation("Sending confirmation email to User ID: {UserId}", order.UserId);

        var user = await _entityRepository.GetAsync<User>(order.UserId);
        var emailOptions = new EmailOptions
        {
            To = [user.Email!],
            Subject = "Your Order Confirmation",
            Body = $"Dear {user.Name},\n\nThank you for your order. Your subscription is now active.\n\nOrder ID: {order.Id}\nPlan: {order.OrderNumber}\nStart Date: {DateTime.UtcNow}\n\nRegards,\nYour Company"
        };

        await _emailService.SendEmailAsync(emailOptions);

        _logger.LogInformation("Confirmation email sent to User ID: {UserId}", order.UserId);
    }

}
