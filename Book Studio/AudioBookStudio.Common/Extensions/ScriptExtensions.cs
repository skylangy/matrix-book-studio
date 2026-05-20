using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Extensions;
public static class ScriptExtensions
{
    public static async Task ExecuteGenerateSubtitle(this IScriptRunner scriptRunner,
        ScriptConfig config,
        string audioFilePath,
        string subtitleFilePath)
    {
        await scriptRunner.ExecuteScriptAsync(config.SubtitleScriptPath,
            $"--mp3_file \"{audioFilePath}\" --srt_file \"{subtitleFilePath}\"");
    }

    public static async Task ExecuteGenerateSubtitleAsync(this IScriptRunner scriptRunner,
       ScriptConfig config,
       string audioFolder,
       string srtFolder)
    {
        await scriptRunner.ExecuteScriptAsync(config.SubtitleScriptPath,
            $"--mp3_folder \"{audioFolder}\" --srt_folder \"{srtFolder}\"");
    }
}
