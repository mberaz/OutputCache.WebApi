namespace OutputCache.OutputCaching
{
    public class OutputCacheKeyCreator
    {
        public static string CreateKey(HttpRequest request)
        {
            return CreateRequestCacheKey(request.Path.ToString(), request.Query.ToDictionary(k => k.ToString(), o => o.ToString()));
        }

        public static string CreateRequestCacheKey(string path, Dictionary<string, string>? queryStringParams = null)
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
