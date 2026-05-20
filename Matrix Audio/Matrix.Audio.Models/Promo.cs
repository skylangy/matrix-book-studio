namespace Matrix.Audio.Models;
public class Promo : Entity
{
    public required string Code { get; set; }
    public required decimal Min { get; set; }
    public required decimal Discount { get; set; }
    public required string DiscountType { get; set; } // e.g., "percentage" or "fixed"
    public required DateTime ValidFrom { get; set; }
    public required DateTime ValidTo { get; set; }
    public required DateTime DateCreated { get; set; } = DateTime.Now;
    public bool IsActive { get; set; }
}
