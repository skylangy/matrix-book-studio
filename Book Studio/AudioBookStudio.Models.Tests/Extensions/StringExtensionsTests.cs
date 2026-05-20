using System.Security;

namespace AudioBookStudio.Models.Tests.Extensions;

public class StringExtensionsTests
{
    [Fact]
    public void TruncateParagraph_ReturnsInputString_WhenInputIsShorterThanMaxLength()
    {
        string input = "This is a short string.";
        int maxLength = 30;

        string output = input.TruncateParagraph(maxLength);

        Assert.Equal(input, output);
    }

    [Fact]
    public void TruncateParagraph_TruncatesToLastCompleteParagraph_WhenInputHasMultipleParagraphs()
    {
        string input = $"This is a long string that needs to be truncated. {Environment.NewLine}It has multiple paragraphs separated by newlines.{Environment.NewLine}The last paragraph should be preserved.";
        int maxLength = 60;

        string output = input.TruncateParagraph(maxLength);

        string expectedOutput = "This is a long string that needs to be truncated. ";
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void TruncateParagraph_TruncatesToLastWord_WhenInputHasNoParagraphBreakWithinMaxLength()
    {
        string input = $"This is a long string that needs to be truncated. {Environment.NewLine}It has no paragraph breaks within the maximum length.";
        int maxLength = 60;

        string output = input.TruncateParagraph(maxLength);

        string expectedOutput = "This is a long string that needs to be truncated. ";
        Assert.Equal(expectedOutput, output);
    }

    [Fact]
    public void TruncateParagraph_ReturnsEmptyString_WhenInputIsEmpty()
    {
        string input = "";
        int maxLength = 10;

        string output = input.TruncateParagraph(maxLength);

        Assert.Equal(input, output);
    }

    [Fact]
    public void ChunkText_ReturnsCorrectChunks()
    {
        // Arrange
        string input = @"Çà¹âÉÁ¶¯£¬Ò»±úÇà¸Ö½£Ù¿µØ´Ì³ö£¬Ö¸ÏòÔÚÄêºº×Ó×ó¼ç£¬Ê¹½£ÉÙÄê²»µÈÕÐÓÃÀÏ£¬Íó¶¶½£Ð±£¬½£·æÒÑÏ÷ÏòÄÇºº×ÓÓÒ¾±¡£ÄÇÖÐÄêºº×Ó½£µ²¸ñ£¬ï£µÄÒ»ÉùÏì£¬Ë«½£Ïà»÷£¬ÎËÎË×÷Éù£¬ÕðÉùÎ´¾ø£¬Ë«½£½£¹â»ô»ô£¬ÒÑ²ðÁËÈýÕÐ£¬ÖÐÄêºº×Ó³¤½£ÃÍµØ»÷Âä£¬Ö±¿³ÉÙÄê¶¥ÃÅ¡£ÄÇÉÙÄê±ÜÏòÓÒ²à£¬×óÊÖ½£¾÷Ò»Òý£¬Çà¸Ö½£¼²´ÌÄÇºº×Ó´óÍÈ¡£ 
¡¡¡¡Á½ÈË½£·¨Ñ¸½Ý£¬È«Á¦Ïà²«¡£ 
¡¡¡¡Á·ÎäÌü¶«×ø×Å¶þÈË¡£ÉÏÊ×ÊÇ¸öËÄÊ®×óÓÒµÄÖÐÄêµÀ¹Ã£¬ÌúÇà×ÅÁ³£¬×ì´½½ô±Õ¡£ÏÂÊ×ÊÇ¸öÎåÊ®ÓàËêµÄÀÏÕß£¬ÓÒÊÖÄí×Å³¤Ðë£¬ÉñÇéÉõÊÇµÃÒâ¡£Á½ÈËµÄ×ùÎ»Ïà¾àÒ»ÕÉÓÐÓà£¬Éíºó¸÷Õ¾×Å¶þÊ®ÓàÃûÄÐÅ®µÜ×Ó¡£Î÷±ßÒ»ÅÅÒÎ×ÓÉÏ×ø×ÅÊ®ÓàÎ»±ö¿Í¡£¶«Î÷Ë«·½µÄÄ¿¹â¶¼¼¯×¢ÓÚ³¡ÖÐ¶þÈËµÄ½Ç¶·¡£ 
¡¡¡¡ÑÛ¼ûÄÇÉÙÄêÓëÖÐÄêºº×ÓÒÑ²ðµ½ÆßÊ®ÓàÕÐ£¬½£ÕÐÔ½À´Ô½½ô£¬Ø£×ÔÎ´·ÖÊ¤°Ü¡£Í»È»ÖÐÄêºº×ÓÒ»½£»Ó³ö£¬ÓÃÁ¦ÃÍÁË£¬Éí×ÓÎ¢Î¢Ò»»Ï£¬ËÆÓûË¤µø¡£Î÷±ß±ö¿ÍÖÐÒ»¸öÉí´©ÇàÉÀµÄÄêÇáÄÐ×ÓÈÌ²»×¡¡°àÍ¡±µÄÒ»ÉùÐ¦¡£ËûËæ¼´ÖªµÀÊ§Ì¬£¬Ã¦ÉìÊÖ°´×¡ÁË¿Ú¡£ 
¡¡¡¡±ãÔÚÕâÊ±£¬³¡ÖÐÉÙÄê×óÊÖºôÒ»ÕÆÅÄ³ö£¬»÷ÏòÄÇºº×ÓºóÐÄ£¬ÄÇºº×ÓÏòÇ°¿ç³öÒ»²½±Ü¿ª£¬ÊÖÖÐ³¤½£ÝëµØÈ¦×ª£¬ºÈÒ»Éù£º¡°×Å£¡¡±ÄÇÉÙÄê×óÍÈÒÑÈ»ÖÐ½££¬ÍÈÏÂÒ»¸öõÔõÄ£¬³¤½£ÔÚµØÏÂÒ»³Å£¬Õ¾Ö±Éí×Ó´ýÓûÔÙ¶·£¬ÄÇÖÐÄêºº×ÓÒÑ»¹½£ÈëÇÊ£¬Ð¦µÀ£º¡°ñÒÊ¦µÜ£¬³ÐÈÃ¡¢³ÐÈÃ£¬ÉËµÃ²»À÷º¦Ã´£¿¡±ÄÇÉÙÄêÁ³É«²Ô°×£¬Ò§×Å×ì´½µÀ£º¡°¶àÐ»¹¨Ê¦ÐÖ½£ÏÂÁôÇé¡£¡± 
¡¡¡¡ÄÇ³¤ÐëÀÏÕßÂúÁ³µÃÉ«£¬Î¢Î¢Ò»Ð¦£¬ËµµÀ£º¡°¶«×ÚÒÑÊ¤ÁËÈýÕó£¬¿´À´Õâ¡®½£ºþ¹¬¡¯ÓÖÒªÈÃ¶«×ÚÔÙ×¡ÎåÄêÁË¡£ÐÁÊ¦ÃÃ£¬ÔÛÃÇ»¹Ðë±ÈÏÂÈ¥Ã´£¿¡±×øÔÚËûÉÏÊ×µÄÄÇÖÐÄêµÀ¹ÃÇ¿ÈÌÅ­Æø£¬ËµµÀ£º¡°×óÊ¦¹ûÈ»µ÷½ÌµÃºÃÍ½¶ù¡£µ«²»Öª×óÊ¦ÐÖ¶Ô¡®ÎÞÁ¿Óñ±Ú¡¯µÄ×êÑÐ£¬ÕâÎåÄêÀ´¿ÉÒÑ´óÓÐÐÄµÃÃ´£¿¡±³¤ÐëÀÏÕßÏòËýµÉÁËÒ»ÑÛ£¬ÕýÉ«µÀ£º¡°Ê¦ÃÃÔõµØÍüÁË±¾ÅÉµÄ¹æ¾Ø£¿¡±ÄÇµÀ¹ÃºßÁËÒ»Éù£¬±ã²»ÔÙËµÏÂÈ¥ÁË¡£ 
¡¡¡¡ÕâÀÏÕßÐÕ×ó£¬Ãû½Ð×ÓÄÂ£¬ÊÇ¡°ÎÞÁ¿½£¡±¶«×ÚµÄÕÆÃÅ¡£ÄÇµÀ¹ÃÐÕÐÁ£¬µÀºÅË«Çå£¬ÊÇ¡°ÎÞÁ¿½£¡±Î÷×ÚÕÆÃÅ¡£";
        int chunkLength = 140;

        // Act
        IEnumerable<string> chunks = input.ChunkText(chunkLength);

        // Assert
        Assert.Equal(14, chunks.Count());
    }

    [Fact]
    public void ChunkText_WithShorterLength_ReturnsWholeText()
    {
        // Arrange
        string input = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec euismod felis nec nunc ullamcorper, non commodo ipsum faucibus.";
        int chunkLength = 200;

        // Act
        IEnumerable<string> chunks = input.ChunkText(chunkLength);

        // Assert
        Assert.Single(chunks);
        Assert.Equal(input, chunks.FirstOrDefault());
    }

    [Fact]
    public void ChunkText_WithEmptyInput_ReturnsEmptyChunks()
    {
        // Arrange
        string input = "";
        int chunkLength = 20;
        string[] expectedChunks = [];

        // Act
        IEnumerable<string> chunks = input.ChunkText(chunkLength);

        // Assert
        Assert.Equal(expectedChunks.Length, chunks.Count());
        Assert.Equal(expectedChunks, chunks);
    }

    [Fact]
    public async Task ReadParagraphsAsync_From_File()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "02　第二章　玉壁月华明.txt");
        var content = await filePath.ReadFileContentAsync();
        var paragraphs = content.ReadParagraphs();

        Assert.Equal(218, paragraphs.Count());
    }

    [Fact]
    public async Task Chunk_Chapter_From_File()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "Chapter1.txt");
        var content = await filePath.ReadFileContentAsync();
        var paragraphs = content.Chunk(2600);

        Assert.Equal(2, paragraphs.Count());
    }

    [Fact]
    public async Task ChunkTextByParagraph_From_File()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "Chapter1.txt");
        var content = await filePath.ReadFileContentAsync();
        var paragraphs = content.ChunkByParagraph(2600);

        Assert.Equal(2, paragraphs.Count());
    }

    [Fact]
    public void XmlContent_Encode_SpecialCharactersAreEscaped()
    {
        // Arrange
        string xmlContent = "<root><tag>Hello & World</tag></root>";

        // Act
        string encodedXml = SecurityElement.Escape(xmlContent);

        // Assert
        string expectedEncodedXml = "&lt;root&gt;&lt;tag&gt;Hello &amp; World&lt;/tag&gt;&lt;/root&gt;";
        Assert.Equal(expectedEncodedXml, encodedXml);
    }

    [Fact]
    public void SplitToSentencesTest()
    {
        var content = @"1936年初东渡黄河作战牺牲。26在中共干部中最有名的朝鲜人。武亭（金武亭），1905年出生于朝鲜咸镜北道镜城郡，1923年流亡中国。先在北平文化大学学习汉语，1924年考入北方军官学校（炮兵科）。1925年加入中国共产党，参加了北伐战争和广州起义。后去上海，在中共上海韩人支部工作。1929年被捕，出狱后转赴香港，又进入江西中央苏区。1934年10月随中央红军长征，任总指挥部作战科长。抗战爆发后担任八路军总部作战科长，深受朱德和彭德怀的信赖。1938年任八路军炮兵团团长，参加过著名的百团大战。1941年组建华北朝鲜青年联合会及朝鲜义勇队。1941年10月作为外国抗日领袖出席在延安召开的东方各民族反法西斯代表大会，其画像与毛泽东像并列挂在会场。抗战胜利后回到朝鲜。27被中共保卫部门秘密枪决的朝鲜人。张志乐（金山），1905年出生于朝鲜平安北道龙川郡，1919年离开朝鲜。1921年考入北京国立协和医科大学，在学期间开始接触中国早期共产主义者，见到过李大钊，结识了瞿秋白，深受影响。1925年在广州加入中国共产党，后进黄埔军校教导团。广州起义失败后到海陆丰苏区，经彭湃介绍加入香港中共党组织活动。1929年初北上，担任中共北平市委组织部部长。1930年12月被捕，并被押送到朝鲜，后因证据不足获释。1931年回到北平后受到当地党组织怀疑，未能恢复党籍。1936年8月，作为朝鲜民族解放同盟代表来到陕甘宁苏区，后安排到红军军政大学任教。1938年8月，因与美国作家尼姆·韦尔斯（埃德加·斯诺的妻子）交往密切，受到延安保卫部门怀疑，未经调查而被秘密处决。28序篇　中朝两党关系的历史渊源转入中共的朝共领导人。李铁夫（韩伟健），1901年出生于朝鲜咸镜南道洪原郡。1919年去俄国，后来中国，一年后到日本早稻田大学读书。1924年回到汉城，1926年12月加入朝鲜共产党（ML派），出席第二次代表大会，并当选为第三届中央委员会委员。朝共组织被破坏后，于1928年初流亡上海，随即加入中国共产党，被派往华北地区工作。1932年担任中共河北省委宣传部长、组织部长。";
        var sentences = content.SplitIntoSentences();

        Assert.Equal(29, sentences.Count());
        Assert.Equal("1936年初东渡黄河作战牺牲。", sentences.ElementAt(0));
        Assert.Equal("1932年担任中共河北省委宣传部长、组织部长。", sentences.LastOrDefault());
    }

    [Fact]
    public async Task RemoveLineBreaks_shouldRemoveLineBreaks()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Books", "ChaterLines.txt");
        var content = await filePath.ReadFileContentAsync();

        var lines = content.ToLines();

        Assert.True(lines.Count() > 1, "Expected more than one line after removing line breaks.");
    }
}