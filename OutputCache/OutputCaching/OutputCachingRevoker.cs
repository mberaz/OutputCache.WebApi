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
    }
}
