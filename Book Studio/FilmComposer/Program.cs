using AudioBookStudio.Films;
using AudioBookStudio.Films.Extensions;
using AudioBookStudio.Films.Models;
using Microsoft.Extensions.Logging;

namespace FilmComposer;

internal class Program
{
    private static ILogger? _logger;
    private readonly static List<BookSnippet> Verses = [
        BookSnippet.ForDay(
            new DateTime(2025, 7, 25),
            @"I:\Source\封面\红楼梦\video\马太福音 51-10.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "马太福音 5:1-10",
            "心灵贫穷的人有福了，\n因为天国是他们的。\n哀痛的人有福了，\n因为他们必得安慰。\n谦和的人有福了，\n因为他们必承受土地。\n爱慕公义如饥似渴的人有福了，\n因为他们必得饱足。\n心存怜悯的人有福了，\n因为他们必蒙上帝的怜悯。\n心灵纯洁的人有福了，\n因为他们必看见上帝。\n使人和睦的人有福了，\n因为他们必被称为上帝的儿女。\n为义受迫害的人有福了，\n因为天国是他们的。",
            "I will refresh and satisfy."),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 26),
            @"I:\Source\封面\红楼梦\video\民数记 6 24–26.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-1.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "民数记 6:24–26",
            "愿耶和华赐福给你，\n保护你。\n愿耶和华使祂的脸光照你，\n赐恩给你。\n愿耶和华向你仰脸，\n赐你平安。",
            "I will refresh and satisfy."),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 27),
            @"I:\Source\封面\红楼梦\video\马太福音 5 13-16.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "马太福音 盐和光 5:13-16",
            "你们是世上的盐。\n如果盐失去咸味，\n怎能使它再变咸呢？\n它将毫无用处，\n只有被丢在外面任人践踏。 \n\n你们是世上的光，\n如同建在山上的城一样无法隐藏。 \n人点亮了灯，\n不会把它放在斗底下，\n而是放在灯台上，\n好照亮全家。  \n同样，\n你们的光也应当照在人面前，\n好让他们看见你们的好行为，\n便赞美你们天上的父。",
            "Our struggle is not against flesh and blood."),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 28),
            @"I:\Source\封面\红楼梦\video\以赛亚书 55 11.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-1.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "以赛亚书 55:11",
            "我口所出的话也必如此，\n必不徒然返回，\n却要成就我所喜悦的，\n在我发他去成就的事上必然亨通。",
            "I will refresh and satisfy."),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 29),
            @"I:\Source\封面\红楼梦\video\以赛亚书 40 31.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-2.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "以赛亚书 40:31",
            "但那等候耶和华的，\n必重新得力。\n他们必如鹰展翅上腾；\n他们奔跑却不困倦，\n行走却不疲乏。",
            "I will refresh and satisfy."),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 30),
            @"I:\Source\封面\红楼梦\video\箴言 3 5–6.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-3.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "箴言 3:5–6",
            "你要专心仰赖耶和华，\n不可倚靠自己的聪明；\n在你一切所行的事上都要认定祂，\n祂必指引你的路。",
            "I will refresh and satisfy.",
            FontNames.MicrosoftYaHei,
            54),
        BookSnippet.ForDay(
            new DateTime(2025, 7, 31),
            @"I:\Source\封面\红楼梦\video\以赛亚书 41 10.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-4.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "以赛亚书 41:10",
            "你不要害怕，\n因为我与你同在；\n不要惊惶，\n因为我是你的神。\n我必坚固你，\n我必帮助你，\n我必用我公义的右手扶持你。",
            "I will refresh and satisfy."),
        BookSnippet.ForDay(
            new DateTime(2025, 8, 1),
            @"I:\Source\封面\红楼梦\video\腓立比书 4 6–7.mp3",
            @"I:\Source\封面\红楼梦\video\ding.mp3",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Bg-5.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Intro.jpg",
            @"I:\Source\封面\红楼梦\video\Youtube-Short-Outro.jpg",
            "Verse of the Day",
            "腓立比书 4:6–7",
            "应当一无挂虑，\n只要凡事藉着祷告、祈求和感谢，\n将你们所要的告诉神。\n神所赐出人意外的平安，\n必在基督耶稣里\n保守你们的心怀意念。",
            "I will refresh and satisfy."),
        ];

    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        _logger ??= LoggerFactory.Create(builder =>
            {
                builder.AddConsole().AddDebug();
            })
            .CreateLogger<CommandBuilder>();

        //await BuildRepeatVideo();
        //BuildFilmWithIntroOutro();
        //await BuildShortVideo();
        var logo = new LogoModel
        {
            ImagePath = @"I:\Source\overlays\bible_logo.png",
            Text = "恩典笔记",
            FontSize = 36,
            ShowShadow = true,
            FontName = FontNames.FZQITT
        };

        foreach (var verse in Verses.Skip(0))
        {
            await FilmBuilder.BuildBibleShortVideo(verse, logo, @$"I:\Source\封面\红楼梦\video\Bible-{verse.Name}.mp4", _logger);
        }
    }

    public static async Task BuildRepeatVideo()
    {
        var settings = new FilmSettings { OutputPath = @"I:\Source\封面\红楼梦\video\第一章-repeat.mp4" };
        var film = FilmBuilder.BuildFilmWithImagesLogo(
            imagesFolder: @"I:\Source\封面\红楼梦\video",
            overlayVideoPath: @"I:\Source\overlays\overlay-5.mp4",
            logoImagePath: @"I:\Source\overlays\book-logo.png",
            nameTexts:
            [
                new("红楼梦 第一回") { FontSize = 28, ShowShadow = true },
                new("甄士隐梦幻识通灵 贾雨村风尘怀闺秀") { FontSize = 22, ShowShadow = true }
            ],
            logoTexts:
            [
                new("深夜书屋") { FontSize = 24, ShowShadow = true }
            ],
            settings: settings
        );
        await film.Export((message) =>
          {
              _logger?.LogInformation("{message}", message);
          });
    }

    public static async Task BuildFilmWithIntroOutro()
    {
        var settings = new FilmSettings { OutputPath = @"I:\Source\封面\红楼梦\video\第一章.mp4" };
        var film = FilmBuilder.BuildFilmWithIntroMainOutro(
            introPath: @"I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg",
            mainVideoPath: @"I:\Source\封面\红楼梦\video\第一章-repeat.mp4",
            outroPath: @"I:\Audio Books\曹雪芹\红楼梦\images\红楼梦-wide-splash.jpg",
            audioPath: @"I:\Audio Books\曹雪芹\红楼梦\mp3\红楼梦-第一章  甄士隐梦幻识通灵 贾雨村风尘怀闺秀.mp3",
            settings: settings
        );

        await film.Export((message) =>
          {
              _logger?.LogInformation("{message}", message);
          });
    }

}

