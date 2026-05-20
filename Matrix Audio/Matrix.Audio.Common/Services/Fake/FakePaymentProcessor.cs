using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Models;

namespace Matrix.Audio.Common.Services.Fake;
public class FakePaymentProcessor : IPaymentProcessor
{
    public Task<string> CreatePaymentIntent(PaymentRequest paymentRequest)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }


    public Task<PaymentResult> ProcessAsync(PaymentConfirmRequest confirmRequest)
    {
        return Task.FromResult(new PaymentResult
        {
            IsSuccess = true,
            TransactionId = Guid.NewGuid().ToString(),
            ErrorMessage = null
        });
    }
}

