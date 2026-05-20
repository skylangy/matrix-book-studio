namespace Matrix.Audio.Server.ViewModels;

public class ResultBase
{
    public bool Success { get; set; }
    public string? Message { get; set; }

    public static ResultBase Empty() => new();

    public static ResultBase Ok(string message = "") =>
        new()
        {
            Success = true,
            Message = message
        };


    public static ResultBase Fail(string message) =>
        new()
        {
            Success = false,
            Message = message
        };
}
