

namespace AudioBookStudio.Films.Tests.Common;
public class FileExtensionsTests
{
    private readonly string _testDir;

    public FileExtensionsTests()
    {
        _testDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDir))
            Directory.Delete(_testDir, recursive: true);
    }

    private string CreateTestFile(string name)
    {
        string fullPath = Path.Combine(_testDir, name);
        File.WriteAllText(fullPath, "test");
        return fullPath;
    }

    [Fact]
    public void EnumerateImageFiles_ShouldReturnOnlySupportedImageFiles_SortedNaturally()
    {
        // Arrange
        CreateTestFile("image_2.jpg");
        CreateTestFile("image_10.jpg");
        CreateTestFile("image_1.png");
        CreateTestFile("notes.txt"); // Not an image

        var dirInfo = new DirectoryInfo(_testDir);

        // Act
        var result = dirInfo.EnumerateImageFiles().ToList();

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().EndWith("image_1.png");
        result[1].Should().EndWith("image_2.jpg");
        result[2].Should().EndWith("image_10.jpg");
    }

    [Fact]
    public void SortNatural_ShouldSortFileInfoNaturally()
    {
        // Arrange
        var f1 = new FileInfo(CreateTestFile("img_1.jpg"));
        var f2 = new FileInfo(CreateTestFile("img_10.jpg"));
        var f3 = new FileInfo(CreateTestFile("img_2.jpg"));

        var files = new List<FileInfo> { f2, f1, f3 };

        // Act
        var result = files.SortNatural().ToList();

        // Assert
        result[0].Should().EndWith("img_1.jpg");
        result[1].Should().EndWith("img_2.jpg");
        result[2].Should().EndWith("img_10.jpg");
    }

    [Fact]
    public void SortNatural_ShouldSortStringsNaturally()
    {
        // Arrange
        var files = new List<string>
        {
            @"C:\temp\img_10.jpg",
            @"C:\temp\img_2.jpg",
            @"C:\temp\img_1.jpg"
        };

        // Act
        var result = files.SortNatural().ToList();

        // Assert
        result[0].Should().EndWith("img_1.jpg");
        result[1].Should().EndWith("img_2.jpg");
        result[2].Should().EndWith("img_10.jpg");
    }
}