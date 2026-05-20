using AudioBookStudio.Common.Abstracts;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System.Collections;
using System.Reflection;

namespace AudioBookStudio.Common.Services;

public static class RepositoryExtensions
{
    public static int Count(this IEnumerable values)
    {
        var count = 0;
        if (values == null)
        {
            return count;
        }

        foreach (var item in values)
        {
            count++;
        }
        return count;
    }

    public static Task<bool> UpdateAsync<T>(this IEntityRepository repository, T entity) where T : Entity
    {
        return repository.UpdateAsync(entity, UpdateEntity);
    }

    private static void UpdateEntity<T>(IAsyncDocumentSession session, T existing, T newModel)
    {
        UpdateProperties(existing, newModel);
    }

    private static void UpdateProperties<T>(T existing, T newModel, params string[] excludedProperties)
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
                if (!AreListsEqual(oldValue as IEnumerable ?? Enumerable.Empty<object>(), newValue as IEnumerable ?? Enumerable.Empty<object>()))
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

    private static bool AreListsEqual(IEnumerable oldList, IEnumerable newList)
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

    public static async Task<Book> GetByNameAsync(this IEntityRepository repository, string name)
    {
        using var session = repository.Store.OpenAsyncSession();
        var book = await session.Query<Book>().Where(x => x.Title == name).FirstOrDefaultAsync();

        return book;
    }

    public static async Task<MediaResource> GetResourceByName(this IEntityRepository repository, string name)
    {
        using var session = repository.Store.OpenAsyncSession();
        var resource = await session.Query<MediaResource>().Where(x => x.Name == name).FirstOrDefaultAsync();
        return resource;
    }

    public static async Task<Author> GetAuthorByNameAsync(this IEntityRepository repository, string name)
    {
        using var session = repository.Store.OpenAsyncSession();
        var author = await session.Query<Author>().Where(x => x.Name == name).FirstOrDefaultAsync();

        return author;
    }

    public static async Task<T> GetByIdAsync<T>(this IEntityRepository repository, string id) where T : Entity
    {
        using var session = repository.Store.OpenAsyncSession();
        var entity = await session.Query<T>().Where(x => x.Id == id).FirstOrDefaultAsync();

        return entity;
    }

    public static async Task<IEnumerable<Tag>> QueryTags(this IEntityRepository repository, IEnumerable<string> tagIds)
    {
        using var session = repository.Store.OpenAsyncSession();
        var tags = await session.Query<Tag>().Where(x => x.Id.In(tagIds)).ToListAsync();
        return tags;
    }

    public static async Task<IEnumerable<Tag>> QueryTags(this IEntityRepository repository, Book book)
    {
        return await repository.QueryTags(book.TagIds);
    }

    public static async Task<IEnumerable<Category>> QueryCategories(this IEntityRepository repository, IEnumerable<string> categoryIds)
    {
        using var session = repository.Store.OpenAsyncSession();
        var categories = await session.Query<Category>().Where(x => x.Id.In(categoryIds)).ToListAsync();
        return categories;
    }

    public static async Task<IEnumerable<Category>> QueryCategories(this IEntityRepository repository, Book book)
    {
        return await repository.QueryCategories(book.CategoryIds);
    }

    public static async Task<IEnumerable<ResultBase>> SyncBooks(this ILibrarySyncer librarySyncer, IEnumerable<string> bookIds)
    {
        var results = new List<ResultBase>();
        foreach (var bookId in bookIds)
        {
            var result = await librarySyncer.SyncBook(bookId);
            results.Add(result);
        }
        return results;
    }

    public static async Task<IEnumerable<ResultBase>> SyncAuthors(this ILibrarySyncer librarySyncer, IEnumerable<string> authorIds)
    {
        var results = new List<ResultBase>();
        foreach (var authorId in authorIds)
        {
            var result = await librarySyncer.SyncAuthor(authorId);
            results.Add(result);
        }
        return results;
    }
}
