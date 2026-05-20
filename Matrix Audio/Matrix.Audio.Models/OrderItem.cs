namespace Matrix.Audio.Models;
public class OrderItem : Entity
{
    public required string OrderId { get; set; }
    public required string ProductId { get; set; }
    public required string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalPriceWithDiscount { get; set; }
}
