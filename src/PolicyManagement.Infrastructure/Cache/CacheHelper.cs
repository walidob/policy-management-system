using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace PolicyManagement.Infrastructure.Cache;

public class CacheHelper : ICacheHelper
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentBag<string> _cacheKeys;

    public CacheHelper(IMemoryCache cache)
    {
        _cache = cache;
        _cacheKeys = [];
    }

    #region IMemoryCache
    public bool TryGetValue<T>(string key, out T value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Set<T>(string key, T value)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(CacheConstants.DefaultCacheDuration);
            
        _cache.Set(key, value, cacheOptions);
        _cacheKeys.Add(key);
    }
    
    public void Set<T>(string key, T value, TimeSpan expiration)
    {
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration);
            
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
    
    public void InvalidateCacheKey(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            _cache.Remove(key);
            var newBag = new ConcurrentBag<string>(_cacheKeys.Where(k => k != key));
            _cacheKeys.Clear();
            foreach (var k in newBag)
            {
                _cacheKeys.Add(k);
            }
        }
    }
    #endregion
} 