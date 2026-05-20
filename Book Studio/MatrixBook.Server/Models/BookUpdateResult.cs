using AudioBookStudio.Models.Data;

namespace MatrixBook.Server.Models;

public class BookUpdateResult
{
    public Book? Book { get; set; }
    public bool Success { get; set; }
}
