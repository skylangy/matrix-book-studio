namespace Matrix.Audio.Models;

public enum SubscriptionTier
{
    Free,
    Premium,
    PremiumPro
}

public enum DiscountType
{
    Percentage,
    Fixed
}

public class SubscriptionPeriod
{
    public required string Name { get; set; }
    public int Months { get; set; }
    public int Days { get; set; }

    public static SubscriptionPeriod Monthly => new()
    {
        Name = "Monthly",
        Months = 1,
        Days = 30
    };

    public static SubscriptionPeriod Semiannual => new()
    {
        Name = "Semiannual",
        Months = 6,
        Days = 183
    };

    public static SubscriptionPeriod Annual => new()
    {
        Name = "Annual",
        Months = 12,
        Days = 366
    };
}

public class SubscriptionPlan : Entity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public SubscriptionTier Tier { get; set; } = SubscriptionTier.Free;
    public SubscriptionPeriod Period { get; set; } = SubscriptionPeriod.Monthly;
    public int Level { get; set; } = 1000;
    public bool IsActive { get; set; } = true;
    /// <summary>
    /// Gets base rate for a month, e.g. $3.99
    /// </summary>
    public required decimal MonthlyRate { get; set; }
    /// <summary>
    /// Gets discount rate for a month
    /// </summary>
    public decimal DiscountRate => ApplyDiscount(MonthlyRate);
    public required DiscountType DiscountType { get; set; } = DiscountType.Percentage;
    /// <summary>
    /// Get or set discount value, percentage: 0 to 1, fixed: or -5
    /// </summary>
    public decimal Discount { get; set; } = 1;
    /// <summary>
    /// Get total amount, e.g. 3.99 * 12
    /// </summary>
    public decimal Total => MonthlyRate * Period.Months;
    /// <summary>
    /// Get total amount after discount, e.g. 3.99 * 12 * 0.8
    /// </summary>
    public decimal TotalAfterDiscount => ApplyDiscount(Total);
    public string Currency { get; set; } = "$";
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

    public List<string> Features { get; set; } = [];


    private decimal ApplyDiscount(decimal value)
    {
        return DiscountType switch
        {
            DiscountType.Percentage => Math.Round(value * Discount, 2),
            DiscountType.Fixed => Math.Round(value + Discount, 2),
            _ => Math.Round(value, 2)
        };
    }
}

