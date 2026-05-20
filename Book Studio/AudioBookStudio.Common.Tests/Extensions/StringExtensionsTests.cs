using AudioBookStudio.Models.Extensions;

namespace AudioBookStudio.Common.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public async Task ReadFileContent_with_correct_encoding_GB2312()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "16　第十六章　昔时因.txt");
        var content = await filePath.ReadFileContent();

        Assert.StartsWith("后一页", content, StringComparison.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task ReadFileContent_with_correct_encoding_UTF8()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "书剑恩仇录.txt");
        var content = await filePath.ReadFileContent();

        Assert.StartsWith("金庸《书剑恩仇录》", content.Trim(), StringComparison.InvariantCultureIgnoreCase);
    }
}
