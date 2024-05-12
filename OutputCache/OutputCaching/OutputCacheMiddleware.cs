using Microsoft.AspNetCore.Http.Features;

namespace OutputCache.OutputCaching
{
    public class OutputCacheMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOutputCachingService _cache;
        private const string OutputCacheKeyHeaderName = "x-output-cache-key";

        private readonly List<int> _goodResponses = new()
        {
            StatusCodes.Status200OK,
            StatusCodes.Status201Created,
            StatusCodes.Status202Accepted,
            StatusCodes.Status204NoContent
        };

        public OutputCacheMiddleware(RequestDelegate next, IOutputCachingService cache)
        {
            _next = next;
            _cache = cache;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;

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

        private static async Task GetFromCache(HttpContext context, OutputCacheResponse cacheResponse)
        {
            // Copy over the HTTP headers
            foreach (var name in cacheResponse.Headers.Keys)
            {
                if (!context.Response.Headers.ContainsKey(name))
                {
                    context.Response.Headers[name] = cacheResponse.Headers[name];
                }
            }
            //add x-output-cache-key header
            context.Response.Headers[OutputCacheKeyHeaderName] = cacheResponse.CacheKey;

            //write the body
            await context.Response.Body.WriteAsync(cacheResponse.Body, 0, cacheResponse.Body.Length);
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

        public bool ShouldSaveResponse(HttpContext context)
        {
            return _goodResponses.Contains(context.Response.StatusCode);
        }
    }
}
