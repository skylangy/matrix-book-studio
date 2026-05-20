namespace AudioBookStudio.Models.Data;
public class ResultBase
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public object? Data { get; set; }

    public static ResultBase Empty() => new();

    public static ResultBase OK(object? data = null, string message = "") =>
        new()
        {
            Success = true,
            Message = message,
            Data = data
        };


    public static ResultBase Fail(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
}