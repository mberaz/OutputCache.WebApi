using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Text;
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
            if (outputCacheAttribute == null || !DoesRequestQualify(context))
            {
                await _next(context);
            }
            else if (_cache.TryGetValue(context.Request, out var response))
            {
                await ServeFromCacheAsync(context, response);
            }
            else
            {
                await ServeFromMvcAndCacheAsync(context, outputCacheAttribute);
            }
        }

        private async Task ServeFromMvcAndCacheAsync(HttpContext context, OutputCacheAttribute outputCacheAttribute)
        {
            HttpResponse response = context.Response;
            Stream originalStream = response.Body;

            try
            {
                using var ms = new MemoryStream();
                response.Body = ms;

                await _next(context);

                if (DoesResponseQualify(context))
                {
                    var bytes = ms.ToArray();

                    AddEtagToResponse(context, bytes);
                    var cacheKey = _cache.Set(context.Request, new OutputCacheResponse(bytes, context.Response.Headers), outputCacheAttribute.DurationInTimeSpan());
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

        private static async Task ServeFromCacheAsync(HttpContext context, OutputCacheResponse value)
        {
            // Copy over the HTTP headers
            foreach (string name in value.Headers.Keys)
            {
                if (!context.Response.Headers.ContainsKey(name))
                {
                    context.Response.Headers[name] = value.Headers[name];
                }
            }

            context.Response.Headers[OutputCacheKeyHeaderName] = value.CacheKey;

            // Serve a conditional GET request when if-none-match header exist
            if (context.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out StringValues etag) && context.Response.Headers[HeaderNames.ETag] == etag)
            {
                context.Response.ContentLength = 0;
                context.Response.StatusCode = StatusCodes.Status304NotModified;
            }
            else
            {
                await context.Response.Body.WriteAsync(value.Body, 0, value.Body.Length);
            }
        }



        private static void AddEtagToResponse(HttpContext context, byte[] bytes)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK)
                return;

            if (context.Response.Headers.ContainsKey(HeaderNames.ETag))
                return;

            context.Response.Headers[HeaderNames.ETag] = CalculateChecksum(bytes, context.Request);
        }

        private static string CalculateChecksum(byte[] bytes, HttpRequest request)
        {
            byte[] encoding = Encoding.UTF8.GetBytes(request.Headers[HeaderNames.AcceptEncoding].ToString());

            using var algo = SHA1.Create();
            byte[] buffer = algo.ComputeHash(bytes.Concat(encoding).ToArray());
            return $"\"{WebEncoders.Base64UrlEncode(buffer)}\"";
        }

        public static bool DoesRequestQualify(HttpContext context)
        {
            if (context.Request.Method != HttpMethods.Get) return false;
            //if (context.User.Identity.IsAuthenticated) return false;

            return true;
        }

        public static bool DoesResponseQualify(HttpContext context)
        {
            if (context.Response.StatusCode != StatusCodes.Status200OK) return false;
            if (context.Response.Headers.ContainsKey(HeaderNames.SetCookie)) return false;

            return true;
        }
    }
}
