using AudioBookStudio.Common.Models;
using AudioBookStudio.Common.Tests.Extensions;
using AudioBookStudio.Models.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Diagnostics;

namespace AudioBookStudio.Common.Tests.Services;
public class TextToVoiceServiceTests
{
    [Fact]
    public async Task Convert_Text_With_Lexicon()
    {
        Environment.SetEnvironmentVariable(Names.SubscriberName, "nnlyx@hotmail.com");
        var configuration = new AppConfiguration()
        {
            BookDatabasePath = "",
            OptionDatabasePath = "",
            RegexLibDatabasePath = "",
            BooksLocation = "",
            DbName = "",
            DbUrl = "",
            Synthesizer = "defaultSynthesizer" // Added required property
        };

        // Create a mock or null logger
        var logger = new Mock<ILogger<TextToVoiceService>>().Object;

        var voiceService = new TextToVoiceService(Options.Create(configuration), null, null, logger);
        var context = new TextToVoiceServiceContext
        {
            UseSsml = true,
            VoiceName = "zh-CN-YunzeNeural",
            Language = "zh-CN",
            OutputPath = AppDomain.CurrentDomain.BaseDirectory,
            FileName = "voice-lexicon.wav".GetResourceFile(),
            SpeechService = "Azure",
        };

        var textFile = "text.txt".GetResourceFile();
        var content = File.ReadAllText(textFile);

        Debug.WriteLine(content);

        var result = await voiceService.Convert(content, context);
        Debug.WriteLine(result.Error);

        Assert.NotNull(result);
        Assert.True(result.Success);
    }
}

