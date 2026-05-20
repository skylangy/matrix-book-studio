namespace AudioBookStudio.Models.Data;

public class ServiceSubscription
{
    public string? Name { get; set; }
    public string? Key { get; set; }
    public string? Region { get; set; }
    public bool IsEnabled { get; set; }
    public int RequestDelayMs { get; set; }
}
