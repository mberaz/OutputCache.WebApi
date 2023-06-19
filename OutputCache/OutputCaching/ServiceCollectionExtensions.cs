using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OutputCache.OutputCaching
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the output caching service with the dependency injection system.
        /// </summary>
        public static void AddOutputCaching(this IServiceCollection services)
        {
            services.AddOutputCaching(new OutputCacheOptions());
        }

        /// <summary>
        /// Registers the output caching service with the dependency injection system.
        /// </summary>
        public static void AddOutputCaching(this IServiceCollection services, Action<OutputCacheOptions> outputCacheOptions)
        {
            var options = new OutputCacheOptions();
            outputCacheOptions(options);

            services.AddOutputCaching(options);
        }

        private static void AddOutputCaching(this IServiceCollection services, OutputCacheOptions options)
        {
            services.AddSingleton(options);
            services.TryAddSingleton<IOutputCachingService, MemoryCachingService>();
            services.TryAddSingleton<IOutputCacheKeysProvider, OutputCacheKeysProvider>();
            services.TryAddSingleton<IOutputCachingRevoker, OutputCachingRevoker>();
        }
    }
}
