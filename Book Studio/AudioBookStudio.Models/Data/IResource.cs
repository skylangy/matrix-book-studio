namespace AudioBookStudio.Models.Data;

public interface IResource
{
    string Id { get; set; }

    string? Type { get; }

    string? Name { get; }

    string? Url { get; }
}
