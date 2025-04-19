using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace PolicyManagement.Infrastructure.Cache;

public class CacheHelper : ICacheHelper
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentBag<string> _cacheKeys;
    private readonly TimeSpan _defaultSlidingExpiration;

    public CacheHelper(IMemoryCache cache)
    {
        _cache = cache;
        _cacheKeys = [];
        _defaultSlidingExpiration = TimeSpan.FromMinutes(15);
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(_defaultSlidingExpiration);
            
        _cache.Set(key, value, cacheOptions);
        _cacheKeys.Add(key);
    }

    public void InvalidateCache()
    {
        foreach (var key in _cacheKeys)
        {
            _cache.Remove(key);
        }
        
        _cacheKeys.Clear();
    }

    public void InvalidateSpecificCache(string key)
    {
        _cache.Remove(key);
    }
} 