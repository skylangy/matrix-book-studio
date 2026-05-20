namespace MatrixBook.Server.Models;

public class DashboardModel
{
    public int BookCount { get; set; }
    public int FinishedBookCount { get; set; }
    public int InProgressBookCount { get; set; }
    public long WordCount { get; set; }
    public long FinishedWordCount { get; set; }
    public long UnfinishedWordCount { get; set; }
    public int AuthorCount { get; set; }
    public List<BookInfo> Books { get; set; } = [];
}
