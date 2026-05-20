namespace Matrix.Audio.Models;
public class OrderProcessResult
{
    public required string OrderId { get; set; }

    public bool IsSuccess { get; set; }

    public string? Message { get; set; }
}
