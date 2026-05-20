using Matrix.Audio.Common.Extensions;
using Matrix.Audio.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

namespace Matrix.Audio.Server.Common;

public static class WebApplicationBuilderExtensions
{
    private const string CorsPolicyName = "AllowAll";

    public static IHostApplicationBuilder ConfigureLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders(); // Optional: Remove default providers if not needed
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();

        Log.Logger = new LoggerConfiguration()
                        .Enrich.With(new ClassNameEnricher())
                        .ReadFrom.Configuration(builder.Configuration)
                        .CreateLogger();

        builder.Logging.AddSerilog();
        return builder;
    }

    public static IHostApplicationBuilder ConfigureCors(this IHostApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
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
        return builder;
    }

    public static IHostApplicationBuilder ConfigureAppSettings(this IHostApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.Configure<AppConfiguration>(configuration.GetSection("AppConfiguration"));
        builder.Services.Configure<PaymentConfig>(configuration.GetSection("PaymentConfig"));
        builder.Services.Configure<JwtConfig>(configuration.GetSection("Jwt"));
        builder.Services.Configure<List<EmailTemplate>>(configuration.GetSection("EmailTemplates"));
        builder.Services.Configure<SmtpConfig>(configuration.GetSection("Smtp"));
        builder.Services.Configure<RedisConfig>(configuration.GetSection("Redis"));
        return builder;
    }

    public static IHostApplicationBuilder ConfigureCaching(this IHostApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.AddMemoryCache();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetRedisUrl();
            options.InstanceName = configuration.GetRedisName();
        });

        services.AddResponseCaching();

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var configuration = ConfigurationOptions.Parse(builder.Configuration.GetRedisUrl(), true);
            configuration.ResolveDns = true;
            return ConnectionMultiplexer.Connect(configuration);
        });

        return builder;
    }

    public static IHostApplicationBuilder ConfigureAuth(this IHostApplicationBuilder builder)
    {
        builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // For authenticating
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;   // For challenges
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration[$"{JwtConfig.JwtConfigKey}:{JwtConfig.IssuerKey}"],
                    ValidAudience = builder.Configuration[$"{JwtConfig.JwtConfigKey}:{JwtConfig.AudienceKey}"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration[$"{JwtConfig.JwtConfigKey}:{JwtConfig.SecretKey}"]!))
                };
            });

        //builder.Services.AddAuthorization(options =>
        //{
        //    options.AddPolicy(UserRoles.Admin, policy => policy.RequireRole(UserRoles.Admin));
        //    options.AddPolicy(UserRoles.User, policy => policy.RequireRole(UserRoles.User));
        //});

        return builder;
    }
}
