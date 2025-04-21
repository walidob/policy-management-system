namespace PolicyManagement.Infrastructure.Cache;

public interface ICacheHelper
{
    #region IMemoryCache
    bool TryGetValue<T>(string key, out T value);
    void Set<T>(string key, T value);
    void Set<T>(string key, T value, TimeSpan expiration);
    void InvalidateCache();
    #endregion

    #region IOutputCache
    Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task InvalidateOutputCache(CancellationToken cancellationToken = default);
    #endregion
}