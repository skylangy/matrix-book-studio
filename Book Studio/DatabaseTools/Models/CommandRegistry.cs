namespace DatabaseTools.Models;
public class CommandRegistry : ICommandRegistry
{
    private readonly Dictionary<string, ICommand> _commands = [];

    public ICommandRegistry Register(ICommand command)
    {
        _commands[command.Name] = command;
        return this;
    }

    public ICommand? GetCommand(string name)
    {
        _commands.TryGetValue(name, out var command);
        return command;
    }

    public void PrintHelp()
    {
        Console.WriteLine("Available commands:");
        foreach (var cmd in _commands.Values)
        {
            Console.WriteLine($"- {cmd.Name}: {cmd.Description}");
        }
    }

}

