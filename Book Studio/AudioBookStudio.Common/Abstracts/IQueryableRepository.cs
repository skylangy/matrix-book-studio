using Raven.Client.Documents.Session;

namespace AudioBookStudio.Common.Abstracts;
public interface IQueryableRepository
{
    Task<IEnumerable<TResult>> QueryAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<TResult> QueryOneAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<IEnumerable<TResult>> Query<TResult>(Func<IDocumentSession, IQueryable<TResult>> query);

    Task<int> CountAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    Task<bool> ExistsAsync<TResult>(Func<IAsyncDocumentSession, IQueryable<TResult>> query);

    IEnumerable<TResult> Load<TResult>(Func<IDocumentSession, IQueryable<TResult>> query);

    IDocumentSession BeginSession();

    IAsyncDocumentSession BeginAsyncSession();
}
