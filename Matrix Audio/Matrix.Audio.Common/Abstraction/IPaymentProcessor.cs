using Matrix.Audio.Models;

namespace Matrix.Audio.Common.Abstraction;
public interface IPaymentProcessor
{
    Task<string> CreatePaymentIntent(PaymentRequest paymentRequest);

    Task<PaymentResult> ProcessAsync(PaymentConfirmRequest confirmRequest);
}
