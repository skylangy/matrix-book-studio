namespace Matrix.Audio.Models;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "usd";
    public string Description { get; set; } = "";
    public List<string> PaymentMethodTypes { get; set; } = ["card"];
}


public class PaymentConfirmRequest : PaymentRequest
{
    public required string TokenId { get; set; }

    public required Order Order { get; set; }
}