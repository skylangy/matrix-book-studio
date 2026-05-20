using LibrarySync;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System.Text;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var serviceProvider = BuildServiceProvider();
        var librarySyncService = serviceProvider.GetRequiredService<ILibrarySyncService>();
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

        string sourcePath = @"G:\Audio Books";
        logger.LogInformation("Source library path: {}", sourcePath);

        string destinationPath = @"D:\Audio Books";
        logger.LogInformation("Destination library path: {}", destinationPath);

        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
        {
            logger.LogInformation("Source and destination paths cannot be empty.");
            return;
        }

        if (!Directory.Exists(sourcePath))
        {
            logger.LogInformation("The source path does not exist.");
            return;
        }

        try
        {
            librarySyncService.SyncLibrary(sourcePath, destinationPath).Wait();
            logger.LogInformation("Library sync completed successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while syncing the library.");
        }
    }

    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider;
    }

    private static void ConfigureServices(ServiceCollection services)
    {
        ConsoleFormatterOptions formatterOptions = new ConsoleFormatterOptions();
        formatterOptions.TimestampFormat = "[HH:mm:ss]";

        services.AddLogging(configure =>
        {
            configure.ClearProviders();
            configure.AddConsole(options =>
            {
                options.FormatterName = "simple";

            });
            configure.AddDebug();
            configure.SetMinimumLevel(LogLevel.Information);
        });

        services.AddTransient<ILibrarySyncService, LibrarySyncService>();
        // Use CopyService for actual copying
        // services.AddTransient<ILibrarySyncService, LogCopyService>(); // Use LogCopyService for testing
    }
}