using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class LinuxFileTransfer(
    IOptions<LibraryConfig> libraryConfig,
    ILogger<LinuxFileTransfer> logger) : FileTransferBase(logger)
{
    private readonly LibraryConfig _libraryConfig = libraryConfig.Value;
    private readonly ILogger<LinuxFileTransfer> _logger = logger;

    public override async Task<bool> Transfer(string soruce, string destination)
    {
        var destFolder = $"{_libraryConfig.Folder}/{destination}".ToLinuxPath();
        var hostname = $"{_libraryConfig.Username}@{_libraryConfig.Hostname}";
        var fullDestination = $"{hostname}:\"{destFolder}/\"".ToLinuxPath();

        _logger.LogInformation("Transferring files from '{Source}' to '{Destination}' on '{Hostname}'", soruce, fullDestination, hostname);
        var sshCommand = "ssh";
        var sshArgs = $"{hostname} sudo mkdir -p '{destFolder}' && sudo chown {_libraryConfig.Username} '{destFolder}'";
        await RunCommand(sshCommand, sshArgs);

        _logger.LogInformation("Running rsync from '{Source}' to '{Destination}'", soruce, fullDestination);
        var command = "rsync";
        var wslArgs = $"-av --progress --partial \"{soruce}\" {fullDestination}";

        _logger.LogInformation("Running command: {Command} with args: {Args}", command, wslArgs);

        return await RunCommand(command, wslArgs);
    }
}
