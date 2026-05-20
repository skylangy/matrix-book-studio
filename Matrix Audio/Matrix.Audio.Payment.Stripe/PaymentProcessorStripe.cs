using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Extensions;
using Matrix.Audio.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace Matrix.Audio.Payment.Stripe;

public class PaymentProcessorStripe : IPaymentProcessor
{
    private readonly PaymentConfig _paymentConfig;
    private readonly PaymentIntentService _paymentIntentService;
    private readonly ILogger<PaymentProcessorStripe> _logger;

    public PaymentProcessorStripe(
        IOptions<PaymentConfig> paymentConfig,
        ILogger<PaymentProcessorStripe> logger)
    {
        _paymentConfig = paymentConfig.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _paymentConfig.SecretKey;
        _paymentIntentService = new PaymentIntentService();
    }

    public async Task<string> CreatePaymentIntent(PaymentRequest paymentRequest)
    {
        var paymentIntent = await _paymentIntentService.CreateAsync(new()
        {
            Amount = Convert.ToInt64(paymentRequest.Amount * 100),
            Currency = paymentRequest.Currency,
            PaymentMethodTypes = paymentRequest.PaymentMethodTypes
        });

        return paymentIntent.ClientSecret;
    }

    public async Task<PaymentResult> ProcessAsync(PaymentConfirmRequest confirmRequest)
    {
        if (_paymentConfig == null || string.IsNullOrEmpty(_paymentConfig.SecretKey))
        {
            const string errorMessage = "Stripe secret key is not configured";
            _logger.LogWarning(errorMessage);
            return PaymentResult.Fail(errorMessage);
        }

        try
        {
            Func<PaymentConfirmRequest, Task<PaymentResult>> action = async (request) => await ProcessPayment(request);
            return await action.RunWithRetry(confirmRequest);
        }
        catch (Exception ex)
        {
            const string errorMessage = "An error occurred while processing the payment";
            _logger.LogError(ex, errorMessage);
            return PaymentResult.Fail(errorMessage);
        }
    }

    private async Task<PaymentResult> ProcessPayment(PaymentConfirmRequest confirmRequest)
    {
        if (_paymentConfig == null || string.IsNullOrEmpty(_paymentConfig.SecretKey))
        {
            return PaymentResult.Fail("Stripe secret key is not configured");
        }
        try
        {
            var paymentIntent = await _paymentIntentService.CreateAsync(new()
            {
                Amount = Convert.ToInt64(confirmRequest.Amount * 100),
                Currency = confirmRequest.Currency,
                PaymentMethodTypes = confirmRequest.PaymentMethodTypes,
                Confirm = true,
                ConfirmationToken = confirmRequest.TokenId
            });
            if (paymentIntent.Status.Equals("succeeded", StringComparison.InvariantCultureIgnoreCase))
            {
                return PaymentResult.Success(paymentIntent.Id);
            }
            else
            {
                return PaymentResult.Fail(paymentIntent.Status);
            }
        }
        catch (Exception ex)
        {
            const string errorMessage = "An error occurred while processing the payment";
            _logger.LogError(ex, errorMessage);
            return PaymentResult.Fail(errorMessage);
        }
    }
}
