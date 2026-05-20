using Matrix.Audio.Server.Common;
using Microsoft.Net.Http.Headers;

internal class Program
{
    private const string CorsPolicyName = "AllowAll";

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        ConfigureApp(builder).Run();
    }

    private static WebApplication ConfigureApp(WebApplicationBuilder builder)
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

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<PreAuthProcessMiddleware>();
        app.UseMiddleware<ExecutionTimeLoggingMiddleware>();
        app.UseMiddleware<RequestKeyMiddleware>();

        app.UseResponseCaching();

        app.Use(async (context, next) =>
        {
            context.Response.GetTypedHeaders().CacheControl =
                new CacheControlHeaderValue()
                {
                    Public = true,
                    MaxAge = TimeSpan.FromSeconds(60)
                };
            context.Response.Headers[HeaderNames.Vary] = "Accept-Encoding";

            await next(context);
        });

        app.MapControllers();
        app.MapFallbackToFile("/index.html");

        return app;
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.ConfigureLogging()
            .ConfigureCors()
            .ConfigureAppSettings()
            .ConfigureCaching()
            .ConfigureAuth();

        var services = builder.Services;
        services.AddDefaultServices();
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }
}