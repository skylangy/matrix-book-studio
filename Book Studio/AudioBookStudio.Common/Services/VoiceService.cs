using AudioBookStudio.Common.Abstracts;

namespace AudioBookStudio.Common.Services;
public class VoiceService : IVoiceService
{
    private readonly List<LanguageVoice> _languages = [];

    public IList<LanguageVoice> GetLanguages()
    {
        if (_languages.Count == 0)
        {
            InitializeLanguages();
        }
        return _languages;
    }

    private void InitializeLanguages()
    {
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin", Value = "zh-CN" },
            Voices = [
            new (){ Name= "Yun Ze", Value = "zh-CN-YunzeNeural"},
            new (){ Name= "Xiao Chen", Value = "zh-CN-XiaochenNeural"},
            new (){ Name= "Xiao Han", Value = "zh-CN-XiaohanNeural"},
            new (){ Name= "Xiao Meng", Value = "zh-CN-XiaomengNeural"   },
            new (){ Name= "Xiao Mo", Value = "zh-CN-XiaomoNeural"},
            new (){ Name= "Xiao Qiu", Value = "zh-CN-XiaoqiuNeural"},
            new (){ Name= "Xiao Rui", Value = "zh-CN-XiaoruiNeural"},
            new (){ Name= "Xiao Shuang", Value = "zh-CN-XiaoshuangNeural"},
            new (){ Name= "Xiao Xiao", Value = "zh-CN-XiaoxiaoNeural"},
            new (){ Name= "Xiao Xuan", Value = "zh-CN-XiaoxuanNeural"},
            new (){ Name= "Xiao Yan", Value = "zh-CN-XiaoyanNeural"},
            new (){ Name= "Xiao Yi", Value = "zh-CN-XiaoyiNeural"},
            new (){ Name= "Xiao You", Value = "zh-CN-XiaoyouNeural"},
            new (){ Name= "Xiao Zhen", Value = "zh-CN-XiaozhenNeural"},
            new (){ Name= "Yun Feng", Value = "zh-CN-YunfengNeural"},
            new (){ Name= "Yun Hao", Value = "zh-CN-YunhaoNeural"},
            new (){ Name= "Yun Jian", Value = "zh-CN-YunjianNeural"},
            new (){ Name= "Yun Xia", Value = "zh-CN-YunxiaNeural"},
            new (){ Name= "Yun Xi", Value = "zh-CN-YunxiNeural" },
            new (){ Name= "Yun Yang", Value = "zh-CN-YunyangNeural"},
            new (){ Name= "Yun Ye", Value = "zh-CN-YunyeNeural"},
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin Henan", Value = "zh-CN-henan" },
            Voices = [
            new (){ Name= "Yun Deng", Value = "zh-CN-henan-YundengNeural"}
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin LiaoNing", Value = "zh-CN-liaoning" },
            Voices = [
            new (){ Name= "Xiao Bei", Value = "zh-CN-liaoning-XiaobeiNeural"}
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin ShanXi", Value = "zh-CN-shaanxi" },
            Voices = [
            new (){ Name= "Xiao Ni", Value = "zh-CN-shaanxi-XiaoniNeural"}
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin ShanDong", Value = "zh-CN-shandong" },
            Voices = [
            new() { Name = "Yun Xiang", Value = "zh-CN-shandong-YunxiangNeural" }
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin SiChuan", Value = "zh-CN-sichuan" },
            Voices = [
            new (){ Name= "Yun Xi", Value = "zh-CN-sichuan-YunxiNeural"}
            ]
        });
        _languages.Add(new()
        {
            Language = new() { Name = "Mandarin TaiWan", Value = "zh-TW" },
            Voices = [
            new (){ Name= "Hsiao Chen", Value = "zh-TW-HsiaoChenNeural"},
            new (){ Name= "Hsiao Yu", Value = "zh-TW-HsiaoYuNeural"},
            new (){ Name= "Yun He", Value = "zh-TW-YunJheNeural"}
            ]
        });
    }
}
