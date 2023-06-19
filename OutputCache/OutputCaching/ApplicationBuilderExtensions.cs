namespace OutputCache.OutputCaching;

public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Registers the output caching middleware
    /// </summary>
    public static void UseOutputCaching(this IApplicationBuilder app)
    {
        app.UseMiddleware<OutputCacheMiddleware>();
    }
}