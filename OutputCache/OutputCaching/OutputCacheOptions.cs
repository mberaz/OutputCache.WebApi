namespace OutputCache.OutputCaching
{
    public class OutputCacheOptions
    {
        public TimeSpan CacheDuration { get; set; } = TimeSpan.FromDays(1);
    }
}
