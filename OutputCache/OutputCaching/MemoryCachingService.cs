using Microsoft.Extensions.Caching.Memory;

namespace OutputCache.OutputCaching
{
    internal class MemoryCachingService : IOutputCachingService
    {
        private IMemoryCache _cache;
        private readonly OutputCacheOptions _cacheOptions;

        public MemoryCachingService(OutputCacheOptions cacheOptions)
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
            _cacheOptions = cacheOptions;
        }

        public bool TryGetValue(HttpRequest request, out OutputCacheResponse response)
        {
            return _cache.TryGetValue(OutputCacheKeyCreator.CreateKey(request), out response);
        }

        public string Set(HttpRequest request, OutputCacheResponse response, TimeSpan? timeSpan)
        {
            response.CacheKey = OutputCacheKeyCreator.CreateKey(request);
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
