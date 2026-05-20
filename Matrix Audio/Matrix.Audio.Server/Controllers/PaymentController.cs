using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Services;
using Matrix.Audio.Models;
using Matrix.Audio.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Matrix.Audio.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentController(
    IOptions<PaymentConfig> paymentConfig,
    IEntityRepository entityRepository,
    IPaymentProcessor paymentProcessor,
    IOrderProcessor orderProcessor,
    ILogger<PaymentController> logger) : ControllerBase
{
    private readonly PaymentConfig _paymentConfig = paymentConfig.Value;
    private readonly IEntityRepository _entityRepository = entityRepository;
    private readonly IPaymentProcessor _paymentProcessor = paymentProcessor;
    private readonly IOrderProcessor _orderProcessor = orderProcessor;
    private readonly ILogger<PaymentController> _logger = logger;


    [HttpGet("key/publishable", Name = "getPublishableApiKey")]
    public IActionResult GetPublishableApiKey()
    {
        return Ok(new PaymentConfig
        {
            PublishableKey = _paymentConfig.PublishableKey,
            SecretKey = ""
        });
    }

    [HttpPost("intent/create", Name = "createPaymentIntent")]
    public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentRequest paymentRequest)
    {
        try
        {
            var clientSecret = await _paymentProcessor.CreatePaymentIntent(paymentRequest);

            return new JsonResult(new { clientSecret });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { error = ex.Message });
        }
    }

    [HttpPost("intent/confirm", Name = "confirmPaymentIntent")]
    public async Task<IActionResult> ConfirmPayment([FromBody] PaymentConfirmRequest paymentRequest)
    {
        try
        {
            _logger.LogInformation("Starting payment processing for Order ID: {OrderId}", paymentRequest.Order.Id);
            var result = await _paymentProcessor.ProcessAsync(paymentRequest);
            if (result.IsSuccess)
            {
                await _entityRepository.UpdateAsync(paymentRequest.Order);
                await _orderProcessor.Process(paymentRequest.Order);
                _logger.LogInformation("Payment processing completed successfully for Order ID: {OrderId}", paymentRequest.Order.Id);
                return Ok(ResultBase.Ok("Payment process succeeded"));
            }
            else
            {
                return BadRequest(ResultBase.Fail("Payment process failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing Order ID: {OrderId}", paymentRequest.Order.Id);
            return BadRequest(ResultBase.Fail(ex.Message));
        }
    }
}
