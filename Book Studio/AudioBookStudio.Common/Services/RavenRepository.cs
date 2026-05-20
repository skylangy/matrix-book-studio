using AudioBookStudio.Common.Extensions;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using System.Collections;
using System.Reflection;

namespace AudioBookStudio.Common.Services;
public abstract class RavenRepository<T>(IDocumentStore documentStore) where T : Entity
{
    protected readonly ModelRepository<T> _repository = new(documentStore);
    protected readonly IDocumentStore _documentStore = documentStore;

    public Task<T> AddAsync(T model)
    {
        return _repository.AddAsync(model);
    }

    public Task<bool> DeleteAsync(string id)
    {
        return _repository.DeleteAsync(id);
    }

    public Task<IEnumerable<T>> GetAllAsync()
    {
        return _repository.GetAsync();
    }

    public Task<T> GetAsync(string id)
    {
        return _repository.GetAsync(id);
    }

    public Task<bool> UpdateAsync(T model)
    {
        return _repository.UpdateAsync(model, UpdateEntity);
    }

    public async Task Imports(IEnumerable<T> models)
    {
        using var bulkInsert = _documentStore.BulkInsert();
        foreach (var model in models)
        {
            await bulkInsert.StoreAsync(model);
        }
    }

    public async Task Empty()
    {
        using var session = _documentStore.OpenAsyncSession();
        await session.Advanced.AsyncDocumentQuery<T>()
            .WaitForNonStaleResults()
            .ToListAsync();

        await _documentStore.Operations
         .SendAsync(new Raven.Client.Documents.Operations.DeleteByQueryOperation(
             $"from {typeof(T).Name}"
         ));
    }

    protected virtual void UpdateEntity(IAsyncDocumentSession session, T existing, T newModel)
    {
        UpdateProperties(existing, newModel);
    }

    protected static void UpdateProperties(T existing, T newModel, params string[] excludedProperties)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Where(p => !excludedProperties.Contains(p.Name));

        foreach (var property in properties)
        {
            if (!property.CanWrite || !property.CanRead)
                continue;

            var oldValue = property.GetValue(existing);
            var newValue = property.GetValue(newModel);

            if (property.PropertyType.IsGenericType && typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
            {
                // Handle List or other collection types
                if (!AreListsEqual(oldValue as IEnumerable, newValue as IEnumerable))
                {
                    property.SetValue(existing, newValue);
                }
            }
            else
            {
                // Handle other property types
                if (!Equals(oldValue, newValue))
                {
                    property.SetValue(existing, newValue);
                }
            }
        }
    }

    protected static bool AreListsEqual(IEnumerable oldList, IEnumerable newList)
    {
        if (oldList == null && newList == null)
            return true;

        if (oldList == null || newList == null)
            return false;

        if (oldList.Count() != newList.Count())
        {
            return false;
        }

        var oldEnumerator = oldList.GetEnumerator();
        var newEnumerator = newList.GetEnumerator();


        while (oldEnumerator.MoveNext() && newEnumerator.MoveNext())
        {
            if (!Equals(oldEnumerator.Current, newEnumerator.Current))
            {
                return false;
            }
        }

        return !oldEnumerator.MoveNext() && !newEnumerator.MoveNext();
    }
}
