namespace OutputCache.OutputCaching
{
    public interface IOutputCachingRevoker
    {
        void Remove(string cacheKey);
    }
}
