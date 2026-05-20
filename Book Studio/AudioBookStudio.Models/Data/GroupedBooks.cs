namespace AudioBookStudio.Models.Data;

public class GroupedBooks
{
    public string? Title { get; set; }
    public IPagedList<Book>? Books { get; set; }
}
