using AudioBookStudio.Common.Abstracts;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AudioBookStudio.Common.Services;
public class EntityRepository(
    IDocumentStore store,
    ILogger<EntityRepository> logger) : IEntityRepository
{
    private readonly IDocumentStore _store = store;
    private readonly ILogger<EntityRepository> _logger = logger;

    public IDocumentStore Store => _store;

    public async Task<bool> DeleteAsync<T>(string id) where T : Entity
    {
        try
        {
            using var session = _store.OpenAsyncSession();
            session.Delete(id);
            await session.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting entity {id}", id);
            return false;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync<T>() where T : Entity
    {
        using var session = _store.OpenAsyncSession();
        return await session.Query<T>().ToListAsync();
    }

    public async Task<T> GetAsync<T>(string id) where T : Entity
    {
        using var session = _store.OpenAsyncSession();
        return await session.LoadAsync<T>(id);
    }

    public async Task<bool> UpdateAsync<T>(T model, Action<IAsyncDocumentSession, T, T> updater) where T : Entity
    {
        try
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating entity {id}", model.Id);
            return false;
        }
    }

    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenAsyncSession();

        var result = await query(session).ToListAsync();

        return result;
    }

    public async Task<TResult> QueryOneAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenAsyncSession();

        return await query(session).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TResult>> Query<TResult>(Func<IDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenSession();
        var queryable = await query(session).ToListAsync();

        return queryable;
    }

    public async Task<int> CountAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenAsyncSession();
        return await query(session).CountAsync();
    }

    public async Task<TResult> Execute<TResult>(Func<IDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenSession();
        var result = query(session).FirstOrDefault();
        return await Task.FromResult(result!);
    }

    public async Task<bool> ExistsAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenAsyncSession();
        return await query(session).AnyAsync();
    }


    public IEnumerable<TResult> Load<TResult>(Func<IDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenSession();
        var queryable = query(session).ToList();

        return queryable;
    }

    public IDocumentSession BeginSession()
    {
        return _store.OpenSession();
    }

    public IAsyncDocumentSession BeginAsyncSession()
    {
        return _store.OpenAsyncSession();
    }
}

