using MatrixBook.Tray.Models;
using MatrixBook.Tray.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MatrixBook.Tray;

internal static class Program
{
    private static Mutex? mMutex;
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        mMutex = new Mutex(true, "MatrixBookStudio.Tray", out bool isNewInstance);
        if (!isNewInstance)
        {
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ApplicationConfiguration.Initialize();

        var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.Production.json", optional: true, reloadOnChange: true)
                .Build();

        Log.Logger = new LoggerConfiguration()
           .MinimumLevel.Debug()
           .WriteTo.Console()
           .WriteTo.File("Logs/app.log", fileSizeLimitBytes: 2 * 1024 * 1024, flushToDiskInterval: TimeSpan.Zero)
           .CreateLogger();

        var serviceProvider = new ServiceCollection()
               .AddLogging(builder =>
               {
                   builder.AddConsole();
                   builder.AddDebug();
                   builder.AddSerilog(dispose: true);
               })
               .Configure<Configuration>(configuration.GetSection(nameof(Configuration)))
               .AddSingleton<IConfiguration>(configuration)
               .AddSingleton<ICommandListener, CommandListener>()
               .AddSingleton<ICommandRunner, ShellCommandRunner>()
               .AddSingleton<IMessageClient, MessageClient>()
               .AddSingleton<IMessageMediator, MessageMediator>()
               .AddSingleton<ICommandHistory, CommandHistory>()
               .AddSingleton<IBackgroundService, QueueProcessingBackgroundService>()
               .AddSingleton<ICommandQueue, CommandQueue>()
               .AddTransient<MainForm>()
               .BuildServiceProvider();

        var mainForm = serviceProvider.GetRequiredService<MainForm>();

        Application.ApplicationExit += (sender, e) =>
        {
            var backgroundService = serviceProvider.GetRequiredService<IBackgroundService>();
            backgroundService.Stop();
            Log.CloseAndFlush();
            mMutex?.Dispose();
        };

        Application.Run(mainForm);
    }
}