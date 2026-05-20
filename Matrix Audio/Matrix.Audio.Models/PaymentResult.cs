namespace Matrix.Audio.Models;
public class PaymentResult
{
    public bool IsSuccess { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }

    public static PaymentResult Empty() =>
        new()
        {
            IsSuccess = false,
            TransactionId = null,
            ErrorMessage = null
        };

    public static PaymentResult Success(string transactionId) =>
        new()
        {
            IsSuccess = true,
            TransactionId = transactionId,
            ErrorMessage = null
        };

    public static PaymentResult Fail(string errorMessage) =>
        new()
        {
            IsSuccess = false,
            TransactionId = null,
            ErrorMessage = errorMessage
        };
}
