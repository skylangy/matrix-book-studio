namespace AudioBookStudio.Models.Data;
public class ChapterBook : Book
{
    public IList<Chapter> Chapters { get; set; } = [];

}
