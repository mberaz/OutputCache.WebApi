namespace OutputCache.OutputCaching
{
    public interface IOutputCachingService
    {
        bool TryGetValue(HttpRequest request, out OutputCacheResponse response);
 
        string Set(HttpRequest request, OutputCacheResponse response, TimeSpan? timeSpan);

        void Clear();
        void Remove(string cacheKey);
    }
}
