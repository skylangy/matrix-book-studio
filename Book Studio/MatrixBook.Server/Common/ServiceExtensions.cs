using AudioBookStudio.Common.Abstracts;
using MatrixBook.Server.Services;
using System.Runtime.InteropServices;

namespace MatrixBook.Server.Common;


public static class ServiceExtensions
{
    public static IServiceCollection UseBookServices(this IServiceCollection services)
    {
        services.AddSingleton<ICommandDispatcher, CommandDispatcher>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //services.AddSingleton<ICommandInvoker, LocalCommandInvoker>();
            services.AddSingleton<ICommandInvoker, SignalRCommandInvoker>();
        }
        else
        {
            services.AddSingleton<ICommandInvoker, SignalRCommandInvoker>();
        }

        return services;
    }
}
