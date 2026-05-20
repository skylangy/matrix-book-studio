using Matrix.Audio.Models;
using Microsoft.Extensions.Configuration;

namespace Matrix.Audio.Common.Extensions;
public static class ConfigurationExtension
{
    public static string[] GetRavenDbUrl(this IConfiguration configuration)
    {
        var url = Environment.GetEnvironmentVariable(EnvVars.RavenDbUrl) ?? configuration["RavenDb:Url"] ?? string.Empty;
        return [url];
    }

    public static string GetRavenDbName(this IConfiguration configuration)
    {
        var name = configuration["RavenDb:DatabaseName"] ?? string.Empty;
        return name;
    }

    public static string GetRedisUrl(this IConfiguration configuration)
    {
        var url = Environment.GetEnvironmentVariable(EnvVars.RedisUrl) ?? configuration["Redis:Url"] ?? string.Empty;
        return url;
    }

    public static string GetRedisName(this IConfiguration configuration)
    {
        var name = configuration["Redis:InstanceName"] ?? string.Empty;
        return name;
    }
}
