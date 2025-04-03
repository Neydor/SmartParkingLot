using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SmartParkingLot.API.Middleware
{
    public class MiddlewareException
    {
        public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
        {
            var deviceId = context.HttpContext.Request.Headers["DeviceId"].FirstOrDefault();
            var rateLimitService = context.HttpContext.RequestServices.GetService<IRateLimitService>();

            if (!Guid.TryParse(deviceId, out var guid) ||
                !rateLimitService.IsAllowed(guid, TimeSpan.FromSeconds(10)))
            {
                context.Result = new StatusCodeResult(429); // Too Many Requests
                return;
            }

            await next();
        }
    }
}
