
using DatabaseTools.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseTools.Commands;
public class GreetCommand(ILogger<GreetCommand> logger) : ICommand
{
    private readonly ILogger<GreetCommand> _logger = logger;

    public string Name => "greet";
    public string Description => "Greets the user. Usage: greet [name]";

    public Task Execute(string[] args)
    {
        var name = args.Length > 0 ? args[0] : "World";
        _logger.LogInformation($"Hello, {name}!");

        return Task.CompletedTask;
    }
}
