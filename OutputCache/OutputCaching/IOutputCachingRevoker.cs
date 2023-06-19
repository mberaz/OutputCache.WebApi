namespace OutputCache.OutputCaching
{
    public interface IOutputCachingRevoker
    {
        void Remove(string cacheKey);
        string CreateRequestCacheKey(HttpMethod httpMethod, string path);
        string CreateRequestCacheKey(string httpMethod, string path);
    }
}
