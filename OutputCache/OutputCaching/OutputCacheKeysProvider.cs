namespace OutputCache.OutputCaching
{
    public class OutputCacheKeysProvider:IOutputCacheKeysProvider
    {
        public string CreateKey(HttpRequest request, string httpMethod = null, string forPath = null)
        {
            return $"{httpMethod ?? request.Method}_{(forPath ?? request.Path).TrimStart('/')}".ToLower();
        }
    }
}
