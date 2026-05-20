using AudioBookStudio.Common.Models;

namespace AudioBookStudio.Common.Abstracts;
public interface IScriptRunner
{
    Task<ScriptResult> ExecuteScriptAsync(string scriptPath, string arguments);
}
