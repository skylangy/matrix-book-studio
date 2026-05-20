using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AudioBookStudio.Common.Services;
public class ModelRepository<T>(IDocumentStore store) where T : Entity
{
    private readonly IDocumentStore _store = store;

    public async Task<T> AddAsync(T model)
    {
        using var session = _store.OpenAsyncSession();
        await session.StoreAsync(model);
        await session.SaveChangesAsync();

        return model;
    }

    public async Task<T> GetAsync(string id)
    {
        using var session = _store.OpenAsyncSession();
        return await session.LoadAsync<T>(id);
    }

    public async Task<IEnumerable<T>> GetAsync()
    {
        using var session = _store.OpenAsyncSession();
        return await session.Query<T>().ToListAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var session = _store.OpenAsyncSession();
        var entity = await session.LoadAsync<object>(id);
        if (entity == null)
        {
            return false;
        }

        session.Delete(id);
        await session.SaveChangesAsync();

        return true;
    }

    public async Task<bool> UpdateAsync(T model, Action<IAsyncDocumentSession, T, T> updater)
    {
        using var session = _store.OpenAsyncSession();
        var existing = await session.LoadAsync<T>(model.Id);
        if (existing == null)
        {
            await session.StoreAsync(model);
        }
        else
        {
            updater?.Invoke(session, existing, model);
            await session.StoreAsync(existing);
        }

        await session.SaveChangesAsync();
        return true;
    }
}

