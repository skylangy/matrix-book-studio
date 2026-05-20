using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AudioBookStudio.Common.Services;
public class ScriptRunner(ILogger<ScriptRunner> logger) : IScriptRunner
{
    private readonly ILogger<ScriptRunner> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<ScriptResult> ExecuteScriptAsync(string scriptPath, string arguments)
    {
        if (string.IsNullOrEmpty(scriptPath))
            throw new ArgumentNullException(nameof(scriptPath));

        var result = new ScriptResult();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = GetInterpreter(scriptPath),
                Arguments = $"\"{scriptPath}\" {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            };

            // Set UTF-8 encoding for Python scripts
            psi.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

            _logger.LogInformation("Executing script: {FileName} {Arguments}",
                psi.FileName, psi.Arguments);

            using var process = new Process { StartInfo = psi };

            process.Start();

            // Read output and errors asynchronously
            process.OutputDataReceived += OutputStandardOutput;
            process.ErrorDataReceived += OutputStandardError;
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            result.Success = process.ExitCode == 0;
            result.ExitCode = process.ExitCode;

            _logger.LogInformation("Script execution completed. Exit code: {ExitCode}", result.ExitCode);

            process.OutputDataReceived -= OutputStandardOutput;
            process.ErrorDataReceived -= OutputStandardError;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute script: {ScriptPath}", scriptPath);
            result.Success = false;
            result.Error = $"Execution failed: {ex.Message}";
            return result;
        }
    }

    private void OutputStandardOutput(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            _logger.LogInformation("[STDOUT] {Message}", e.Data);
        }
    }

    private void OutputStandardError(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            _logger.LogWarning("[STDERR] {Message}", e.Data);
        }
    }

    private string GetInterpreter(string scriptPath)
    {
        string extension = System.IO.Path.GetExtension(scriptPath).ToLower();
        return extension switch
        {
            ".py" => "python",
            ".ps1" => "powershell",
            ".bat" => "cmd",
            _ => throw new ArgumentException($"Unsupported script type: {extension}")
        };
    }
}