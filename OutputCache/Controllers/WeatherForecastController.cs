using Microsoft.AspNetCore.Mvc;
using OutputCache.OutputCaching;

namespace OutputCache.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly IOutputCachingRevoker _outputCachingRevoker;
        private static readonly string[] Summaries = 
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherForecastController(IOutputCachingRevoker outputCachingRevoker)
        {
            _outputCachingRevoker = outputCachingRevoker;
        }

        [HttpGet("get/{id}/{temp}")]
        [OutputCache(durationInSeconds: 300)]
        public string Get(int id, int temp)
        {
            return Summaries[id] + "_" + temp;
        }
 
        [HttpGet("remove/{id}/{temp}")]
        public string Remove(int id, int temp)
        {
            var key = OutputCacheKeyCreator.CreateRequestCacheKey($"WeatherForecast/get/{id}/{temp}");
            _outputCachingRevoker.Remove(key);

            return key;
        }

        [HttpGet("random/{id}/{temp}")]

        public string Random(int id, int temp)
        {
            return Summaries[id] + "_" + temp;
        }
    }
}