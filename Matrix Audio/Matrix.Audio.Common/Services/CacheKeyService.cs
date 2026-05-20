using Matrix.Audio.Common.Abstraction;

namespace Matrix.Audio.Common.Services;
public class CacheKeyService : ICacheKeyService
{
    public string Create(string prefix, params object[] parts)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            throw new ArgumentNullException(nameof(prefix));

        var keyParts = parts
            .Where(p => p != null)
            .Select(p => p.ToString()?.Trim().ToLowerInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s));

        var key = string.Join(":", new[] { prefix }.Concat(keyParts));
        return key;
    }
}
