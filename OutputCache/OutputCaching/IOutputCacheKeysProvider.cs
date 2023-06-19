namespace OutputCache.OutputCaching
{
    public interface IOutputCacheKeysProvider
    {
        string CreateKey(HttpRequest request, string httpMethod = null, string forPath = null);
    }
}
