using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Extensions;
using Matrix.Audio.Common.Services.RavenDb.Index;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace Matrix.Audio.Common.Services.RavenDb;
public static class ServiceExtensions
{
    public static IServiceCollection UseRavenRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentStore>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var store = new DocumentStore
            {
                Urls = configuration.GetRavenDbUrl(),
                Database = configuration.GetRavenDbName()
            };
            store.Initialize();
            store.InitializeIndexes();
            return store;
        });

        services.AddSingleton<IEntityRepository, EntityRepository>();

        return services;
    }

    public static void InitializeIndexes(this IDocumentStore store)
    {
        new AlbumsByTags().Execute(store);
        new AlbumsByCategories().Execute(store);
        new TotalDurationIndex().Execute(store);
    }
}
