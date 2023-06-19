﻿using Microsoft.AspNetCore.Mvc.Filters;

namespace OutputCache.OutputCaching
{
    public class OutputCacheAttribute : ActionFilterAttribute
    {
        public OutputCacheAttribute()
        {

        }

        public OutputCacheAttribute(int durationInSeconds)
        {
            DurationInSeconds = durationInSeconds;
        }
        public int? DurationInSeconds { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

        }
    }
}
