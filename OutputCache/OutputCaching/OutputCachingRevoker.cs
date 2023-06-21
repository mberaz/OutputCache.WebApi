namespace OutputCache.OutputCaching
{
    public class OutputCachingRevoker : IOutputCachingRevoker
    {
        private readonly IOutputCachingService _outputCachingService;

        public OutputCachingRevoker(IOutputCachingService outputCachingService)
        {
            _outputCachingService = outputCachingService;
        }

        public void Remove(string cacheKey)
        {
            _outputCachingService.Remove(cacheKey);
        }

        public string CreateRequestCacheKey(string path, Dictionary<string, string>? queryStringParams = null)
        {
            var key = path.TrimStart('/');

            if (queryStringParams != null)
            {
                key = queryStringParams.Keys.OrderBy(o => o)
                    .Aggregate(key, (current, qaKey) => current + ("_" + qaKey + "=" + queryStringParams[qaKey]));
            }
            return key.ToLower();
        }
    }
}
