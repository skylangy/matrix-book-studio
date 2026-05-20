namespace Matrix.Audio.Common.Abstraction;
public interface IOnlineUserTrackService
{
    Task UpdateUserActivityAsync(string userId);

    Task<IEnumerable<string>> GetOnlineUsersAsync();

    Task<bool> IsUserOnlineAsync(string userId);

    Task<int> GetOnlineUserCountAsync();
}
