namespace Matrix.Audio.Models;
public class PaymentInfo
{
    public string CardNumber { get; set; } = string.Empty;
    public string CardHolderName { get; set; } = string.Empty;
    public string ExpirationDate { get; set; } = string.Empty; // Format: MM/YY
    public string Cvv { get; set; } = string.Empty;
}

