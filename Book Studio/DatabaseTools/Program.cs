using DatabaseTools.Commands;
using DatabaseTools.Models;
using DatabaseTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

class Program
{
    static void Main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);

        var serviceProvider = serviceCollection.BuildServiceProvider();


        var commandRegistry = serviceProvider.GetRequiredService<ICommandRegistry>();

        if (args.Length == 0)
        {
            Console.WriteLine("No command provided. Use 'help' to see available commands.");
            commandRegistry.PrintHelp();
            return;
        }

        var commandName = args[0] ?? "lite-to-raven";
        var command = commandRegistry.GetCommand(commandName);

        if (command == null)
        {
            Console.WriteLine($"Unknown command: {commandName}");
            commandRegistry.PrintHelp();
            return;
        }

        command.Execute(args.Skip(1).ToArray()).Wait();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole(); // Logs to console
            configure.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<CommandRegistry>();

        // Register commands
        services.AddSingleton<ICommand, GreetCommand>();
        services.AddSingleton<ICommand, SumCommand>();
        services.AddSingleton<ICommand, BookToAudioCommand>();
        services.AddSingleton<ICommand, LiteDbToRavenDbCommand>();

        // Add commands to the registry
        services.AddSingleton<ICommandRegistry>(provider =>
        {
            var registry = provider.GetRequiredService<CommandRegistry>();
            var commands = provider.GetServices<ICommand>();
            foreach (var command in commands)
            {
                registry.Register(command);
            }
            return registry;
        });
    }
}
