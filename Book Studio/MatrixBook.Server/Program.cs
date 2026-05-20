using AudioBookStudio.Common.Extensions;
using MatrixBook.Server.Common;
using MatrixBook.Server.Services;
using Microsoft.Net.Http.Headers;

internal class Program
{
    private const string CorsPolicyName = "AllowAll";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

        ConfigureServices(builder);

        var app = Configure(builder);

        app.Run();
    }

    public static void ConfigureServices(IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddCors(options =>
        {
            options.AddPolicy(CorsPolicyName, builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(host => true)
                    ;
            });
        });

        builder.UseLogging();

        services.UseConfigurations(configuration);

        services.AddMemoryCache();
        services.AddResponseCaching();

        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<IOptionsService, OptionsService>();
        services.AddSingleton<IBookExportService, BookExportService>();

        services.UseDefaultBookServices()
                .UseBookServices();

        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
    }

    public static WebApplication Configure(WebApplicationBuilder builder)
    {
        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors(CorsPolicyName);
        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.UseResponseCaching();

        app.Use(async (context, next) =>
        {
            context.Response.GetTypedHeaders().CacheControl =
                new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(10)
                };
            context.Response.Headers[HeaderNames.Vary] = "Accept-Encoding";

            await next(context);
        });

        app.UseMiddleware<ExecutionTimeLoggingMiddleware>();

        app.MapControllers();

        app.MapHub<WorkProgressHub>("/workprogress");
        app.MapHub<CommandHub>("/command");
        app.MapFallbackToFile("/index.html");

        return app;
    }
}