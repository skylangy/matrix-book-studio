using AudioBookStudio.Models.Data;

namespace AudioBookStudio.Models.Tests.Data;

public class SrtTests
{
    [Fact]
    public void SrtLine_ToString_ShouldFormatCorrectly()
    {
        var line = new SrtLine
        {
            Index = 1,
            Start = TimeSpan.FromSeconds(1),
            End = TimeSpan.FromSeconds(4),
            TextLines = ["Hello world", "Second line"]
        };

        string expected =
            "1\n00:00:01,000 --> 00:00:04,000\nHello world\nSecond line\n";

        Assert.Equal(expected.Replace("\n", Environment.NewLine), line.ToString());
    }

    [Fact]
    public void SrtLine_Shift_ShouldAdjustTimes()
    {
        var line = new SrtLine
        {
            Index = 1,
            Start = TimeSpan.FromSeconds(5),
            End = TimeSpan.FromSeconds(10),
            TextLines = ["Hello!"]
        };

        line.Shift(TimeSpan.FromSeconds(3));

        Assert.Equal(TimeSpan.FromSeconds(8), line.Start);
        Assert.Equal(TimeSpan.FromSeconds(13), line.End);
    }

    [Fact]
    public void SrtFile_Load_ShouldParseCorrectly()
    {
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, @"
1
00:00:00,000 --> 00:00:03,560
是指千中非贵，年过七十长夕。

2
00:00:04,179 --> 00:00:07,839
福明身后有谁知，万世空花游戏。

3
00:00:08,640 --> 00:00:11,960
修成少年狂荡，莫贪花酒便宜。

4
00:00:12,640 --> 00:00:15,500
脱礼烦恼是何非，随分安心得意。

".Trim().Replace("\n", Environment.NewLine));

        var srt = SrtFile.Load(tempFile);

        Assert.Equal(4, srt.Lines.Count);
        Assert.Equal("是指千中非贵，年过七十长夕。", srt.Lines[0].TextLines[0]);
        Assert.Equal("脱礼烦恼是何非，随分安心得意。", srt.Lines[3].TextLines[0]);

        File.Delete(tempFile);
    }

    [Fact]
    public void SrtFile_ShiftAll_ShouldUpdateTimes()
    {
        var srt = new SrtFile
        {
            Lines =
            [
                new SrtLine
                {
                    Index = 1,
                    Start = TimeSpan.FromSeconds(0),
                    End = TimeSpan.FromSeconds(2),
                    TextLines = ["First"]
                }
            ]
        };

        srt.ShiftAll(TimeSpan.FromSeconds(10));

        Assert.Equal(TimeSpan.FromSeconds(10), srt.Lines[0].Start);
        Assert.Equal(TimeSpan.FromSeconds(12), srt.Lines[0].End);
    }

    [Fact]
    public void SrtFile_Save_ShouldCreateValidOutput()
    {
        var tempFile = Path.GetTempFileName();

        var srt = new SrtFile
        {
            Lines =
            [
                new SrtLine
                {
                    Index = 1,
                    Start = TimeSpan.FromSeconds(1),
                    End = TimeSpan.FromSeconds(3),
                    TextLines = ["Test line"]
                }
            ]
        };

        srt.Save(tempFile);

        string output = File.ReadAllText(tempFile);

        Assert.Contains("00:00:01,000 --> 00:00:03,000", output);
        Assert.Contains("Test line", output);

        File.Delete(tempFile);
    }

    [Fact]
    public void Load_srt_from_file()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "喻世明言-第一章 蒋兴哥重会珍珠衫.srt");
        var srt = SrtFile.Load(filePath);
        Assert.NotNull(srt);
        Assert.NotEmpty(srt.Lines);
        Assert.Equal(1136, srt.Lines.Count);
    }
}
