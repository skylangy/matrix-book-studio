namespace MatrixBook.Server.Models;

public class BookSearchModel
{
    public string? Status { get; set; }
    public string? Keyword { get; set; }
    public string? SortBy { get; set; }
    public string? ThenBy { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool? NoImage { get; set; }
}
