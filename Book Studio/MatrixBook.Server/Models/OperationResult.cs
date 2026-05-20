namespace MatrixBook.Server.Models;

public class OperationResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Payload { get; set; }
}
