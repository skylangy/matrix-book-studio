namespace DatabaseTools.Models;
public interface ICommandRegistry
{
    ICommandRegistry Register(ICommand command);

    ICommand? GetCommand(string name);
    void PrintHelp();
}

