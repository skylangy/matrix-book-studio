namespace DatabaseTools.Models;
public interface ICommand
{
    Task Execute(string[] args);
    string Name { get; }
    string Description { get; }
}
