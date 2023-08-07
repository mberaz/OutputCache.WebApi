using Microsoft.AspNetCore.Http.Features;

namespace OutputCache.OutputCaching
{
    public class OutputCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOutputCachingService _cache;
        private const string OutputCacheKeyHeaderName = "x-output-cache-key";

        public OutputCacheMiddleware(RequestDelegate next, IOutputCachingService cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>().Endpoint;

            var outputCacheAttribute = endpoint?.Metadata.GetMetadata<OutputCacheAttribute>();
            if (outputCacheAttribute == null || context.Request.Method != HttpMethods.Get)
            {
                await _next(context);
            }
            else if (_cache.TryGetValue(context.Request, out var response))
            {
                await GetFromCache(context, response);
            }
            else
            {
                await InvokeAndSaveResponse(context, outputCacheAttribute);
            }
        }

        private async Task InvokeAndSaveResponse(HttpContext context, OutputCacheAttribute outputCacheAttribute)
        {
            HttpResponse response = context.Response;
            Stream originalStream = response.Body;

            try
            {
                using var ms = new MemoryStream();
                response.Body = ms;

                await _next(context);

                if (ShouldSaveResponse(context))
                {
                    var cacheKey = _cache.Set(context.Request, new OutputCacheResponse(ms.ToArray(), context.Response.Headers), outputCacheAttribute.DurationInTimeSpan());
                    context.Response.Headers[OutputCacheKeyHeaderName] = cacheKey;
                }

                if (ms.Length > 0)
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(originalStream);
                }
            }
            finally
            {
                response.Body = originalStream;
            }
        }

        private static async Task GetFromCache(HttpContext context, OutputCacheResponse value)
        {
            // Copy over the HTTP headers
            foreach (string name in value.Headers.Keys)
            {
                if (!context.Response.Headers.ContainsKey(name))
                {
                    context.Response.Headers[name] = value.Headers[name];
                }
            }
            //add x-output-cache-key header
            context.Response.Headers[OutputCacheKeyHeaderName] = value.CacheKey;

            //write the body
            await context.Response.Body.WriteAsync(value.Body, 0, value.Body.Length);
        }

        public static bool ShouldSaveResponse(HttpContext context)
        {
            var goodResponses = new List<int>
            {
                StatusCodes.Status200OK,
                StatusCodes.Status202Accepted,
                StatusCodes.Status201Created,
                StatusCodes.Status204NoContent
            };

            return goodResponses.Contains(context.Response.StatusCode);
        }
    }
}
