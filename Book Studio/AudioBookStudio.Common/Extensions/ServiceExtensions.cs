using AudioBookStudio.Common.Abstracts;
using AudioBookStudio.Common.Services;
using AudioBookStudio.Common.Services.Fake;
using AudioBookStudio.Common.Services.Index;
using AudioBookStudio.Common.Shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using Raven.Client.Documents;
using System.Runtime.InteropServices;

namespace AudioBookStudio.Common.Extensions;

public static class ServiceExtensions
{
    private static readonly bool _useFakeServices = false;

    public static IServiceCollection UseDefaultBookServices(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentStore>(provider =>
        {
            var configuration = provider.GetRequiredService<IOptions<AppConfiguration>>().Value;
            var store = new DocumentStore
            {
                Urls = [configuration.DbUrl],
                Database = configuration.DbName
            };
            store.Initialize();
            store.InitializeIndexes();
            return store;
        });

        services.AddHostedService<BackgroundWorkerService>();
        services.AddSingleton<IQueryableRepository, QueryableRepository>();
        services.AddSingleton<IDataImporter, RavenDataImporter>();
        services.AddSingleton<IEntityRepository, EntityRepository>();
        services.AddSingleton<IWorkProgressService, WorkProgressService>();
        services.AddSingleton<IVoiceService, VoiceService>();
        services.AddSingleton<IWorkProgressRepository, WorkProgressRepository>();
        services.AddSingleton<IBackgroundWorkerQueue, BackgroundWorkerQueue>();
        services.AddSingleton<IJobGenerator, JobGenerator>();
        services.AddSingleton<IContentWriter, ContentWriter>();
        services.AddSingleton<IBookImageCache, BookImageCache>();
        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<IMetadataProcessor, MetadataProcessor>();
        services.AddSingleton<ILibrarySyncer, LibrarySyncer>();
        services.AddSingleton<IVideoExportService, VideoExportService>();

        services.AddSingleton<IScriptRunner, ScriptRunner>();
        services.AddSingleton<ISynthesizePreProcessor, SynthesizePreProcessor>();
        services.AddSingleton<AzureSynthesizer>();
        services.AddSingleton<EdgeSynthesizer>();
        services.AddTransient<Func<string, ISpeechSynthesizer>>(serviceProvider => key =>
        {
            return key switch
            {
                "Azure" => serviceProvider.GetRequiredService<AzureSynthesizer>(),
                "Edge" => serviceProvider.GetRequiredService<EdgeSynthesizer>(),
                _ => throw new ArgumentException($"No synthesizer found for {key}")
            };
        });

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services.AddSingleton<IFileTransfer, WslFileTransfer>();
            services.AddSingleton<IPathMappingService, LocalPathMappingService>();
        }
        else
        {
            services.AddSingleton<IFileTransfer, LinuxFileTransfer>();
            services.AddSingleton<IPathMappingService, LinuxPathMappingService>();
        }

        if (_useFakeServices)
        {
            services.AddSingleton<ITextToVoiceService, FakeTextToVoiceService>();
            services.AddSingleton<IWavToMp3Converter, FakeWavToMp3Converter>();
            services.AddSingleton<IVideoGenerator, FakeVideoGenerator>();
            services.AddSingleton<ISubtitleGenerator, FakeSubtitleGenerator>();
        }
        else
        {
            services.AddSingleton<ITextToVoiceService, TextToVoiceService>();
            services.AddSingleton<IWavToMp3Converter, WavToMp3Converter>();
            services.AddSingleton<IVideoGenerator, VideoGenerator>();
            services.AddSingleton<ISubtitleGenerator, TimeSubtitleGenerator>();
            services.AddSingleton<IVideoCombiner, VideoCombiner>();
        }

        return services;
    }

    public static void InitializeIndexes(this IDocumentStore store)
    {
        try
        {
            new BooksByTags().Execute(store);
            new BooksByCategories().Execute(store);
            new BooksByCategoryAndCount().Execute(store);
            new BooksByTagAndCount().Execute(store);
            new BooksByStatusAndCount().Execute(store);
            new BooksByAuthorAndCount().Execute(store);
            new BookByStatus().Execute(store);
        }
        catch (Exception ex)
        {
            throw new Exception("Initialize Database failed.", ex);
        }
    }

    public static void UseLogging(this IHostApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        builder.Logging.AddConsoleFormatter<SimpleConsoleFormatter, SimpleConsoleFormatterOptions>();
        builder.Logging.AddConsole(options =>
        {
            options.FormatterName = "short";
        });
        //builder.Logging.AddDebug();
        builder.Logging.AddProvider(new CustomDebugLoggerProvider());
    }
}
