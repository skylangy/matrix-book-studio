
using AudioBookStudio.Common.Abstracts;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace AudioBookStudio.Common.Services;
public class QueryableRepository(IDocumentStore store)
    : IQueryableRepository
{
    private readonly IDocumentStore _store = store;


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

    public Task<bool> ExistsAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
    {
        using var session = _store.OpenAsyncSession();
        return query(session).AnyAsync();
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
