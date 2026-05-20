using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FilmCS
{
    public class FontFile
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public FontFile(string name, string path)
        {
            Name = name;
            Path = path;
        }
        public string EscapePath()
        {
            if (string.IsNullOrEmpty(Path)) return "";
            return Path.Replace("\\", "/").Replace(":", "\\:").Replace("\"", "\\\"");
        }
        public override string ToString() => $"FontFile(name=\"{Name}\", path=\"{Path}\")";
        public bool Exists() => File.Exists(Path);
    }

    public static class Fonts
    {
        private static readonly string WindowsFontPath = Environment.ExpandEnvironmentVariables("%WINDIR%\\Fonts");
        private static readonly HashSet<string> FontExtensions = new HashSet<string> { ".ttf", ".otf", ".ttc" };
        private static readonly Dictionary<string, FontFile> FontMap = new Dictionary<string, FontFile>();
        private static bool _initialized = false;
        private static readonly Dictionary<string, string> PredefinedFonts = new Dictionary<string, string>
        {
            { "Arial", Path.Combine(WindowsFontPath, "arial.ttf") },
            { "MicrosoftYaHei", Path.Combine(WindowsFontPath, "msyh.ttc") },
            { "SimHei", Path.Combine(WindowsFontPath, "simhei.ttf") },
            { "SimSun", Path.Combine(WindowsFontPath, "simsun.ttc") },
            { "KaiTi", Path.Combine(WindowsFontPath, "simkai.ttf") },
            { "FangSong", Path.Combine(WindowsFontPath, "simfang.ttf") },
            { "TimesNewRoman", Path.Combine(WindowsFontPath, "times.ttf") },
            { "PingFang", "/System/Library/Fonts/PingFang.ttc" },
            { "Heiti SC", "/System/Library/Fonts/STHeiti Light.ttc" },
            { "DejaVuSans", "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf" },
            { "DejaVuSans-Bold", "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf" },
            { "NotoSansCJK", "/usr/share/fonts/opentype/noto/NotoSansCJK-Regular.ttc" },
            { "WenQuanYiZenHei", "/usr/share/fonts/wenquanyi/wqy-zenhei.ttc" }
        };

        private static List<string> GetDefaultFontDirs()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new List<string> { WindowsFontPath };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return new List<string> { "/System/Library/Fonts", "/Library/Fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library/Fonts") };
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return new List<string> { "/usr/share/fonts", "/usr/local/share/fonts", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts") };
            return new List<string>();
        }

        private static void Initialize()
        {
            if (_initialized) return;
            foreach (var fontDir in GetDefaultFontDirs())
            {
                if (!Directory.Exists(fontDir)) continue;
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

        public static FontFile Resolve(string name)
        {
            Initialize();
            return FontMap.ContainsKey(name) ? FontMap[name] : null;
        }
        public static void Register(string name, string path)
        {
            FontMap[name] = new FontFile(name, path);
        }
        public static void Unregister(string name)
        {
            if (FontMap.ContainsKey(name))
                FontMap.Remove(name);
        }
        public static List<string> List()
        {
            Initialize();
            return FontMap.Keys.OrderBy(x => x).ToList();
        }
        public static Dictionary<string, FontFile> Dump()
        {
            Initialize();
            return new Dictionary<string, FontFile>(FontMap);
        }
    }
}
