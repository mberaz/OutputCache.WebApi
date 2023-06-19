using Microsoft.AspNetCore.Mvc;

namespace OutputCache.OutputCaching;

public static class ControllerContextExtensions
{
    public static string GetControllerName(this ControllerBase controllerBase)
    {
        return controllerBase.ControllerContext.RouteData.Values["controller"].ToString();
    }
}