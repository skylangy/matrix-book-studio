using DatabaseTools.Models;
using Microsoft.Extensions.Logging;

namespace DatabaseTools.Commands;
public class SumCommand(ILogger<SumCommand> logger) : ICommand
{
    private readonly ILogger<SumCommand> _logger = logger;

    public string Name => "sum";
    public string Description => "Calculates the sum of two numbers. Usage: sum [num1] [num2]";

    public Task Execute(string[] args)
    {
        if (args.Length < 2)
        {
            _logger.LogInformation("Usage: sum [num1] [num2]");
            return Task.CompletedTask;
        }

        if (int.TryParse(args[0], out var num1) && int.TryParse(args[1], out var num2))
        {
            _logger.LogInformation($"The sum is: {num1 + num2}");
        }
        else
        {
            _logger.LogInformation("Invalid numbers provided.");
        }

        return Task.CompletedTask;
    }
}
