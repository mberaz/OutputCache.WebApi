namespace OutputCache.OutputCaching
{
    public class OutputCacheKeyCreator
    {
        public static string CreateKey(HttpRequest request)
        {
            var key = $"{(request.Path.ToString()).TrimStart('/')}";

            if (request.Query.Any())
            {
                key = request.Query.Keys.OrderBy(o => o)
                    .Aggregate(key, (current, qaKey) => current + ("_" + qaKey + "=" + request.Query[qaKey]));
            }
            return key.ToLower();
        }
    }
}
