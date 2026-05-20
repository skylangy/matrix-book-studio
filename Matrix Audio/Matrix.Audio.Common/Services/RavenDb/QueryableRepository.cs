namespace Matrix.Audio.Common.Services.RavenDb;
//public class QueryableRepository(IDocumentStore store)
//    : IQueryableRepository
//{
//    private readonly IDocumentStore _store = store;


//    public async Task<IEnumerable<TResult>> QueryAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
//    {
//        using var session = _store.OpenAsyncSession();

//        var result = await query(session).ToListAsync();

//        return result;
//    }

//    public async Task<TResult> QueryOneAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
//    {
//        using var session = _store.OpenAsyncSession();

//        return await query(session).FirstOrDefaultAsync();
//    }

//    public async Task<IEnumerable<TResult>> Query<TResult>(Func<IDocumentSession, IQueryable<TResult>> query)
//    {
//        using var session = _store.OpenSession();
//        var queryable = await query(session).ToListAsync();

//        return queryable;
//    }

//    public async Task<int> CountAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query)
//    {
//        using var session = _store.OpenAsyncSession();
//        return await query(session).CountAsync();
//    }

//}
