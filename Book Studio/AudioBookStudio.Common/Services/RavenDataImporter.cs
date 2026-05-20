using AudioBookStudio.Common.Abstracts;
using Raven.Client.Documents;

namespace AudioBookStudio.Common.Services;
public class RavenDataImporter(IDocumentStore documentStore) : IDataImporter
{
    private readonly IDocumentStore _documentStore = documentStore;

    public async Task Import<T>(IEnumerable<T> items) where T : class
    {
        using var bulkInsert = _documentStore.BulkInsert();
        foreach (var album in items)
        {
            await bulkInsert.StoreAsync(album);
        }
    }
}
