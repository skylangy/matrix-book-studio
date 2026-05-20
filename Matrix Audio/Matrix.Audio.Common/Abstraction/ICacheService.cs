namespace Matrix.Audio.Common.Abstraction;
public interface ICacheService
{
    Task<T> GetAsync<T>(string key);

    Task<string> GetStringAsync(string key);

    Task SetAsync<T>(string key, T value, int durationInMinutes = 15);

    Task SetStringAsync(string key, string value, int durationInMinutes = 15);

    Task RemoveAsync(string key);

    Task RemoveByPatternAsync(string prefix);
}
