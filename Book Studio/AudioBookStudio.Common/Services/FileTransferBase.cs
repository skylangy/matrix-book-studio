using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace AudioBookStudio.Common.Services;
public abstract class FileTransferBase(ILogger logger) : IFileTransfer
{
    private readonly ILogger _logger = logger;

    public abstract Task<bool> Transfer(string source, string destination);


    protected async Task<bool> RunCommand(string command, string arg)
    {
        ProcessStartInfo processStartInfo = new()
        {
            FileName = command,
            Arguments = arg,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        void OutputHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger.LogInformation("Output: {Data}", e.Data);
        }

        void ErrorHandler(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
                _logger.LogError("Error: {Data}", e.Data);
        }

        _logger.LogInformation("Running command with: '{FileName}' '{Arguments}'", processStartInfo.FileName, processStartInfo.Arguments);

        using Process process = new();
        process.StartInfo = processStartInfo;
        process.OutputDataReceived += OutputHandler;
        process.ErrorDataReceived += ErrorHandler;

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.LogError("Run command {FileName} with error {ExitCode}", processStartInfo.FileName, process.ExitCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run command: {FileName} {Argument} with {Exception}", processStartInfo.FileName, processStartInfo.Arguments, ex.Message);
            return false;
        }
        finally
        {
            process.OutputDataReceived -= OutputHandler;
            process.ErrorDataReceived -= ErrorHandler;
        }

        return true;
    }

}
