using Matrix.Audio.Common.Abstraction;
using Matrix.Audio.Common.Extensions;
using Matrix.Audio.Common.Services.RavenDb.Index;
using Matrix.Audio.Models;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using System.Collections;
using System.Reflection;

namespace Matrix.Audio.Common.Services;
public static class RepositoryExtensions
{
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

    public static Task<Post> GetPostBySlugAsync(this IEntityRepository repository, string slug)
    {
        return repository.QueryOneAsync(session => session.Query<Post>().Where(x => x.Slug == slug));
    }

    public static Task<bool> IsSlugUsed(this IEntityRepository repository, string slug)
    {
        return repository.QueryOneAsync(session => session.Query<Post>().Where(x => x.Slug == slug))
                         .ContinueWith(task => task.Result != null);
    }

    public static Task<IEnumerable<SubscriptionPlan>> GetPlansFromConfig(this IEntityRepository _, IConfiguration configuration)
    {
        var section = configuration.GetSection("Plans");
        var subscriptionPlans = section.Get<List<SubscriptionPlan>>() ?? [];

        return Task.FromResult(subscriptionPlans.AsEnumerable());
    }

    public static Task PopulatePlanFromConfig(this IEntityRepository entityRepository, IConfiguration configuration)
    {
        return entityRepository.GetPlansFromConfig(configuration)
                               .ContinueWith(task => entityRepository.Imports(task.Result));
    }

    public static async Task Imports<T>(this IEntityRepository repository, IEnumerable<T> entities)
    {
        using var bulkInsert = repository.Store.BulkInsert();
        foreach (var entity in entities)
        {
            await bulkInsert.StoreAsync(entity);
        }
    }

    public static async Task<bool> DeleteUserOperationAsync(this IEntityRepository repository, string userId, string albumId, string operation)
    {
        var userOperation = await repository.QueryOneAsync(session => session.Query<UserOperation>()
                                                                   .Where(x => x.UserId == userId && x.TargetId == albumId && x.Operation == operation))
                        ;
        if (userOperation != null)
        {
            return await repository.DeleteAsync<UserOperation>(userOperation.Id);
        }

        return false;
    }

    public static async Task<EmailSubscribe> GetSubscriptionByEmailAsync(this IEntityRepository repository, string email)
    {
        return await repository.QueryOneAsync(session => session.Query<EmailSubscribe>().Where(x => x.Email == email));
    }

    public static async Task<UserSettings> GetSettingsForUserAsync(this IEntityRepository repository, string userId)
    {
        var settings = await repository.QueryOneAsync(session => session.Query<UserSettings>().Where(x => x.UserId == userId));

        return settings;
    }

    public static async Task<User> GetUserByEmailAsync(this IEntityRepository repository, string email)
    {
        return await repository.QueryOneAsync(session => session.Query<User>().Where(x => x.Email == email));
    }

    public static async Task<UserProfile> GetProfileForUserAsync(this IEntityRepository repository, string userId)
    {
        return await repository.QueryOneAsync(session => session.Query<UserProfile>().Where(x => x.UserId == userId));
    }

    public static async Task<int> GetUserPlayAlbumCountAsyn(this IEntityRepository repository, string userId)
    {
        var count = await repository.CountAsync(session => session.Query<UserOperation>()
                                                                                 .Where(x => x.UserId == userId && x.Operation == UserOperationType.PlayAlbum)
                                                                                 .Select(x => x.TargetId)
                                                                                 .Distinct()
                                                                                 );
        return count;
    }

    public static async Task<UserLogin> GetUserLogin(this IEntityRepository entityRepository, string userId)
    {
        return await entityRepository.QueryOneAsync(session => session.Query<UserLogin>().Where(x => x.UserId == userId));
    }

    public static async Task<UserLogin> GetUserLoginByEmail(this IEntityRepository entityRepository, string email)
    {
        return await entityRepository.QueryOneAsync(session => session.Query<UserLogin>().Where(x => x.UserEmail == email));
    }

    public static Task<IEnumerable<AppSetting>> GetAppSettingsFromConfigAsync(this IEntityRepository _, IConfiguration configuration)
    {
        var section = configuration.GetSection("AppSettings");
        var appSettings = new List<AppSetting>();
        foreach (var child in section.GetChildren())
        {
            appSettings.Add(new AppSetting
            {
                Id = child.Key,
                Name = child.Key,
                Value = child.Value!
            });
        }
        return Task.FromResult(appSettings.AsEnumerable());
    }

    public static Task PopulateAppSettingsFromConfigAsync(this IEntityRepository repository, IConfiguration configuration)
    {
        return repository.GetAppSettingsFromConfigAsync(configuration)
                         .ContinueWith(task => repository.Imports(task.Result));
    }

    public static async Task<IEnumerable<Episode>> GetAlbumEpisodesAsync(this IEntityRepository repository, string albumId)
    {
        return await repository.QueryAsync(session => session.Query<Episode>().Where(x => x.AlbumId == albumId));
    }

    public static async Task<UserSubscription> GetUserSubscriptionAsync(this IEntityRepository repository, string userId)
    {
        return await repository.QueryOneAsync(session => session.Query<UserSubscription>()
                                                                .Where(x => x.UserId == userId));
    }

    public static async Task<double> GetTotalDurationInHour(this IEntityRepository repository)
    {
        var result = await repository.Execute(session => session.Query<TotalDurationIndex.Result, TotalDurationIndex>());
        var duration = result?.TotalDuration ?? 0;

        return duration / 1000 / 60 / 60;
    }

    public static async Task<Album> GetAlbumByTitleAndArtist(this IEntityRepository repository, string title, string artist)
    {
        return await repository.QueryOneAsync(session => session.Query<Album>()
                                                                .Where(x => x.Title == title && x.Artist == artist));
    }

    public static async Task<Artist> GetArtistByName(this IEntityRepository repository, string name)
    {
        return await repository.QueryOneAsync(session => session.Query<Artist>().Where(x => x.Name == name));
    }

    public static async Task<ImageResource> GetImageByName(this IEntityRepository repository, string fileName, string folderName)
    {
        return await repository.QueryOneAsync(session => session.Query<ImageResource>()
                                                                .Where(x => x.FileName == fileName && x.FolderName == folderName));
    }

    public static async Task<IEnumerable<Tag>> GetTagsByName(this IEntityRepository repository, IEnumerable<string> tagNames)
    {
        var nameSet = tagNames.ToHashSet();
        var tags = await repository.QueryAsync(session => session.Query<Tag>()
                                                                 .Where(x => x.Name.In(nameSet)));
        return tags;
    }

    public static async Task<IEnumerable<Category>> GetCategoriesByName(this IEntityRepository repository, IEnumerable<string> categoryNames)
    {
        var nameSet = categoryNames.ToHashSet();
        var categories = await repository.QueryAsync(session => session.Query<Category>()
                                                                       .Where(x => x.Name.In(nameSet)));
        return categories;
    }

    public static async Task<Episode> GetEpisodeByTitleAndAlbum(this IEntityRepository repository, string title, string albumid)
    {
        return await repository.QueryOneAsync(session => session.Query<Episode>()
                                                                .Where(x => x.Title == title && x.AlbumId == albumid));
    }
}
