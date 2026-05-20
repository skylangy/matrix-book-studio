namespace Matrix.Audio.Models;
public class Order : Entity
{
    public required string UserId { get; set; }
    public required string OrderType { get; set; }
    public required string OrderStatus { get; set; }
    public required string OrderNumber { get; set; }
    public required string Currency { get; set; } = "USD";
    public string? Description { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; }

    public List<OrderItem> Items { get; set; } = [];
    public Address BillingAddress { get; set; } = new();
    public PaymentInfo PaymentInfo { get; set; } = new();
    public Promo? Promo { get; set; }
}
