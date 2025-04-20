namespace PolicyManagement.Infrastructure.Cache;

public interface ICacheHelper
{
    bool TryGetValue<T>(string key, out T value);
    void Set<T>(string key, T value);
    void Set<T>(string key, T value, TimeSpan expiration);
    void InvalidateCache();
    void InvalidateSpecificCache(string key);
} 