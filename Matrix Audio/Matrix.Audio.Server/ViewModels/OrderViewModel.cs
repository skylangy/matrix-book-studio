namespace Matrix.Audio.Server.ViewModels;

public class OrderViewModel
{
    public required string OrderId { get; set; }
    public required string UserId { get; set; }
    public required string OrderNumber { get; set; }
    public required string OrderStatus { get; set; }
    public required string ProductName { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public DateTime DateCreated { get; set; }
}
