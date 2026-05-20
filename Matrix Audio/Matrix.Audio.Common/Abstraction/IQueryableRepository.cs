using Raven.Client.Documents.Session;

namespace Matrix.Audio.Common.Abstraction;
public interface IQueryableRepository
{
    Task<IEnumerable<TResult>> QueryAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<TResult> QueryOneAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<IEnumerable<TResult>> Query<TResult>(Func<IDocumentSession, IQueryable<TResult>> query);

    Task<int> CountAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<TResult> Execute<TResult>(Func<IDocumentSession, IQueryable<TResult>> query);
}
