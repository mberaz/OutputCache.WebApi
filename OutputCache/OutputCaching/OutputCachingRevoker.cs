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

        public string CreateRequestCacheKey(HttpMethod httpMethod, string path)
        {
            return CreateRequestCacheKey(httpMethod.ToString(), path);
        }

        public string CreateRequestCacheKey(string httpMethod, string path)
        {
            return $"{httpMethod}_{path.TrimStart('/')}".ToLower();
        }


    }
}
