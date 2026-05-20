using System.Runtime.InteropServices;

namespace AudioBookStudio.Films.Models;

public static class Fonts
{
    private static readonly string WindowsFontPath = Environment.ExpandEnvironmentVariables("%WINDIR%\\Fonts");
    private static readonly string UserFontPath = Environment.ExpandEnvironmentVariables(@"%USERPROFILE%\AppData\Local\Microsoft\Windows\Fonts");
    private static readonly HashSet<string> FontExtensions = [".ttf", ".otf", ".ttc"];
    private static readonly Dictionary<string, FontFile> FontMap = [];
    private static bool _initialized = false;
    private static readonly Dictionary<string, string> PredefinedFonts = new()
    {
            { FontNames.Arial, Path.Combine(WindowsFontPath, "arial.ttf") },
            { FontNames.MicrosoftYaHei, Path.Combine(WindowsFontPath, "msyh.ttc") },
            { FontNames.SimHei, Path.Combine(WindowsFontPath, "simhei.ttf") },
            { FontNames.SimSun, Path.Combine(WindowsFontPath, "simsun.ttc") },
            { FontNames.KaiTi, Path.Combine(WindowsFontPath, "simkai.ttf") },
            { FontNames.FangSong, Path.Combine(WindowsFontPath, "simfang.ttf") },
            { FontNames.TimesNewRoman, Path.Combine(WindowsFontPath, "times.ttf") },
            { FontNames.AgencyFB, Path.Combine(WindowsFontPath, "agencyr.ttf") },
            { FontNames.Pin8, Path.Combine(UserFontPath, "8-PM___.ttf") },
            { FontNames.Inkburrow, Path.Combine(UserFontPath, "INKBURRO.ttf") },
            { FontNames.Amazone, Path.Combine(UserFontPath, "AMAZONEN.ttf") },
            { FontNames.FZQITT, Path.Combine(UserFontPath, "FZQITT.ttf") },
            { FontNames.FZQITS, Path.Combine(UserFontPath, "fzqtjw_0.ttf") },
            { FontNames.BauhausBT, Path.Combine(UserFontPath, "BAUHAUSL.ttf") },
            { FontNames.BernhardMod, Path.Combine(UserFontPath, "BNHRDMON.ttf") },
            { FontNames.BernhardTango, Path.Combine(UserFontPath, "BNHRDTAN.ttf")   }
        };

    private static List<string> GetDefaultFontDirs()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return [WindowsFontPath, UserFontPath];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return ["/System/Library/Fonts", "/Library/Fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts")];
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return ["/usr/share/fonts", "/usr/local/share/fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")];
        return [];
    }

    private static void Initialize()
    {
        if (_initialized)
            return;

        foreach (var fontDir in GetDefaultFontDirs())
        {
            if (!Directory.Exists(fontDir))
                continue;

            foreach (var file in Directory.EnumerateFiles(fontDir, "*.*", SearchOption.AllDirectories))
            {
                var ext = Path.GetExtension(file).ToLower();
                if (FontExtensions.Contains(ext))
                {
                    var fontFile = new FontFile(Path.GetFileNameWithoutExtension(file), file);
                    if (!FontMap.ContainsKey(fontFile.Name))
                        FontMap[fontFile.Name] = fontFile;

                }
            }
        }
        foreach (var kv in PredefinedFonts)
        {
            if (File.Exists(kv.Value))
                FontMap[kv.Key] = new FontFile(kv.Key, kv.Value);
        }
        _initialized = true;
    }

    public static FontFile? Resolve(string name)
    {
        Initialize();
        FontMap.TryGetValue(name, out FontFile? fontFile);
        return fontFile;
    }

    public static void Register(string name, string path)
    {
        FontMap[name] = new FontFile(name, path);
    }

    public static void Unregister(string name)
    {
        FontMap.Remove(name);
    }

    public static List<string> List()
    {
        Initialize();
        return [.. FontMap.Keys.OrderBy(x => x)];
    }
}