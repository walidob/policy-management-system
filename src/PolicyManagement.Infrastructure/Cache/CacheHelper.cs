using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace PolicyManagement.Infrastructure.Cache;

public class CacheHelper : ICacheHelper
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentBag<string> _cacheKeys;
    private readonly TimeSpan _defaultSlidingExpiration;
    private readonly IOutputCacheStore _outputCacheStore;

    public CacheHelper(IMemoryCache cache, IOutputCacheStore outputCacheStore)
    {
        _cache = cache;
        _cacheKeys = [];
        _defaultSlidingExpiration = TimeSpan.FromMinutes(15);
        _outputCacheStore = outputCacheStore;
    }

    #region IMemoryCache
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
    #endregion


    #region IOutputCache
    public async Task EvictByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await _outputCacheStore.EvictByTagAsync(tag, cancellationToken);
        
        InvalidateCache();
    }
    
    public async Task InvalidateOutputCache(CancellationToken cancellationToken = default)
    {
        await EvictByTagAsync(CacheConstants.PoliciesTag, cancellationToken);
    }
    #endregion
} 