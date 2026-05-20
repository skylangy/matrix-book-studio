using AudioBookStudio.Common.Models;
using AudioBookStudio.Models.Data;

namespace AudioBookStudio.Common.Tests.Services;
public class WorkProgressTests
{
    [Fact]
    public void GetInProgressItemsTest()
    {
        var workProgressItems = new List<WorkProgress>
        {
            new() { Id="", Category = "Audio", Name = "Chapter 1", Status = BookStatus.InProgress, Timestamp = DateTime.Now.AddHours(-1) },
            new() { Id="", Category = "Audio", Name = "Chapter 1", Status = BookStatus.InProgress, Timestamp = DateTime.Now.AddHours(-2) },
            new() { Id="", Category = "Audio", Name = "Chapter 2", Status = BookStatus.InProgress, Timestamp = DateTime.Now.AddHours(-3) },
            new() { Id="", Category = "Video", Name = "Chapter 1", Status = BookStatus.InProgress, Timestamp = DateTime.Now.AddHours(-4) }
        };

        var result = workProgressItems
           .Where(x => x.Status == BookStatus.InProgress)
           .GroupBy(x => (x.Category, x.Name))
           .Select(g => g.OrderByDescending(x => x.Timestamp).First())
           .OrderByDescending(x => x.Timestamp)
           .ToList();

        // Output the result for verification
        foreach (var item in result)
        {
            Console.WriteLine($"WorkProgress: {item.Category}, {item.Name}, {item.Timestamp}");
        }

        // Assert the result count
        Assert.Equal(3, result.Count);
    }
}
