namespace OutputCache.OutputCaching
{
    public interface IOutputCachingRevoker
    {
        void Remove(string cacheKey);
        string CreateRequestCacheKey( string path, Dictionary<string,string>? queryStringParams=null);
    }
}
