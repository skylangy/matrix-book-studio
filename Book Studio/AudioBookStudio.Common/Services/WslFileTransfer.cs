using AudioBookStudio.Models.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudioBookStudio.Common.Services;
public class WslFileTransfer(
    IOptions<LibraryConfig> libraryConfig,
    ILogger<WslFileTransfer> logger) : FileTransferBase(logger)
{
    private readonly LibraryConfig _libraryConfig = libraryConfig.Value;
    private readonly ILogger<WslFileTransfer> _logger = logger;

    public override async Task<bool> Transfer(string soruce, string destination)
    {
        var destFolder = $"{_libraryConfig.Folder}/{destination}/".ToLinuxPath();
        var hostname = $"{_libraryConfig.Username}@{_libraryConfig.Hostname}";
        var fullDestination = $"{hostname}:\"{destFolder}/\"".ToLinuxPath();
        var wslSource = soruce.ToWslPath();

        var sshCommand = "ssh";
        var sshArgs = $"{hostname} sudo mkdir -p '{destFolder}' && sudo chown {_libraryConfig.Username} '{destFolder}'";
        await RunCommand(sshCommand, sshArgs);

        var wslCommand = "wsl";
        var wslArgs = $"rsync -av --progress --partial {wslSource} {fullDestination}";

        return await RunCommand(wslCommand, wslArgs);
    }
}
