using Microsoft.Extensions.Caching.Memory;

namespace OutputCache.OutputCaching
{
    internal class MemoryCachingService : IOutputCachingService
    {
        private IMemoryCache _cache;
        private readonly IOutputCacheKeysProvider _cacheKeysProvider;
        private readonly OutputCacheOptions _cacheOptions;

        public MemoryCachingService(
            IOutputCacheKeysProvider cacheKeysProvider, OutputCacheOptions cacheOptions)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cacheKeysProvider = cacheKeysProvider;
            _cacheOptions = cacheOptions;
        }

        public bool TryGetValue(HttpContext context, out OutputCacheResponse response)
        {
            return _cache.TryGetValue(_cacheKeysProvider.CreateKey(context.Request), out response);
        }

        public string Set(HttpContext context, OutputCacheResponse response, TimeSpan? timeSpan)
        {
            response.CacheKey = _cacheKeysProvider.CreateKey(context.Request);
            _cache.Set(response.CacheKey, response, absoluteExpirationRelativeToNow: timeSpan ?? _cacheOptions.CacheDuration);
            return response.CacheKey;
        }

        public void Clear()
        {
            var old = Interlocked.Exchange(ref _cache, new MemoryCache(new MemoryCacheOptions()));
            old?.Dispose();
        }

        public void Remove(string cacheKey)
        {
            _cache.Remove(cacheKey);
        }
    }
}
