using MatrixBook.Server.Models;
using Raven.Client.Documents;

namespace MatrixBook.Server.Services;

public class OptionsService(IDocumentStore documentStore) : IOptionsService
{
    private readonly IDocumentStore _documentStore = documentStore;

    public async Task<OptionCollection> GetOptionsAsync()
    {
        using var session = _documentStore.OpenAsyncSession();
        var options = await session.Query<Option>().ToListAsync();

        return new() { Items = options };
    }

    public async Task<OptionCollection> UpdateOptionsAsync(OptionCollection options)
    {
        using var session = _documentStore.OpenAsyncSession();

        foreach (var option in options.Items)
        {
            await session.StoreAsync(option);
        }
        await session.SaveChangesAsync();

        var collection = await GetOptionsAsync();

        return collection;
    }
}
