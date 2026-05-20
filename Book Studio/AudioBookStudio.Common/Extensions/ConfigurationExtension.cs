using AudioBookStudio.Common.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AudioBookStudio.Common.Extensions;
public static class ConfigurationExtension
{
    public static IServiceCollection UseConfigurations(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ScriptConfig>(configuration.GetSection(nameof(ScriptConfig)));
        services.Configure<AppConfiguration>(configuration.GetSection(nameof(AppConfiguration)));
        services.Configure<LibraryConfig>(configuration.GetSection(nameof(LibraryConfig)));
        services.Configure<VideoCombineConfig>(configuration.GetSection(nameof(VideoCombineConfig)));
        services.Configure<AzureSynthesizerConfig>(configuration.GetSection(nameof(AzureSynthesizerConfig)));
        services.Configure<EdgeSynthesizerConfig>(configuration.GetSection(nameof(EdgeSynthesizerConfig)));
        services.Configure<SynthesizePreProcessConfig>(configuration.GetSection(nameof(SynthesizePreProcessConfig)));
        services.Configure<SpeechConfiguration>(configuration.GetSection(nameof(SpeechConfiguration)));

        return services;
    }
}
