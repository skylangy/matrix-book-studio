namespace AudioBookStudio.Films.Tests.Common;
public class NaturalStringComparerTests
{
    private readonly NaturalStringComparer _comparer = new();

    [Theory]
    [InlineData("image_1.jpg", "image_2.jpg", -1)]
    [InlineData("image_10.jpg", "image_2.jpg", 1)]
    [InlineData("image_5.jpg", "image_5.jpg", 0)]
    [InlineData("file_2.txt", "file_10.txt", -1)]
    [InlineData("file_20.txt", "file_3.txt", 1)]
    public void Compare_ShouldReturnExpectedOrder(string a, string b, int expectedSign)
    {
        var result = _comparer.Compare(a, b);

        // Normalize result to -1, 0, 1
        int normalized = result == 0 ? 0 : result < 0 ? -1 : 1;

        normalized.Should().Be(expectedSign);
    }

    [Fact]
    public void Sort_ListOfFilenames_ShouldBeInNaturalOrder()
    {
        var input = new List<string>
        {
            "image_1.jpg",
            "image_10.jpg",
            "image_2.jpg",
            "image_20.jpg",
            "image_11.jpg",
            "image_3.jpg"
        };

        var expected = new List<string>
        {
            "image_1.jpg",
            "image_2.jpg",
            "image_3.jpg",
            "image_10.jpg",
            "image_11.jpg",
            "image_20.jpg"
        };

        input.Sort(_comparer);
        input.Should().Equal(expected);
    }

    [Fact]
    public void Compare_ShouldFallbackToLength_IfTextAndNumbersAreEqual()
    {
        var shorter = "item_1";
        var longer = "item_1_extra";

        _comparer.Compare(shorter, longer).Should().BeLessThan(0);
    }
}
