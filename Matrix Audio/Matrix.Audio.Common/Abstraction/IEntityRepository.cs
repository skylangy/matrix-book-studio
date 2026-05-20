using Matrix.Audio.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Matrix.Audio.Common.Abstraction;
public interface IEntityRepository : IQueryableRepository
{
    IDocumentStore Store { get; }

    Task<T> GetAsync<T>(string id) where T : Entity;

    Task<IEnumerable<T>> GetAllAsync<T>() where T : Entity;

    Task<bool> DeleteAsync<T>(string id) where T : Entity;

    Task<bool> UpdateAsync<T>(T model, Action<IAsyncDocumentSession, T, T> updater) where T : Entity;
}
