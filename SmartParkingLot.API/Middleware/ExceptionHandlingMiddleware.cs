using SmartParkingLot.Application.Exceptions;
using System.Text.Json;

namespace SmartParkingLot.API.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred processing {Path}: {ErrorMessage}", context.Request.Path, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = StatusCodes.Status500InternalServerError;
            var title = "An unexpected error occurred.";
            var detail = "An internal server error has occurred. Please try again later.";

            switch (exception)
            {
                case NotFoundException e:
                    statusCode = StatusCodes.Status404NotFound;
                    title = "Resource Not Found";
                    detail = e.Message; 
                    break;
                case ValidationException e:
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Validation Error";
                    detail = e.Message; 
                    if (e.Message.Contains("rate limit exceeded", StringComparison.OrdinalIgnoreCase))
                    {
                        statusCode = StatusCodes.Status429TooManyRequests;
                        title = "Rate Limit Exceeded";
                    }
                    break;
                case ConflictException e:
                    statusCode = StatusCodes.Status409Conflict;
                    title = "Conflict Error";
                    detail = e.Message; 
                    break;
                case ArgumentException e: 
                    statusCode = StatusCodes.Status400BadRequest;
                    title = "Invalid Argument";
                    detail = e.Message;
                    break;
            }

            context.Response.ContentType = "application/problem+json"; 
            context.Response.StatusCode = statusCode;

            var problemDetails = new 
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = context.Request.Path 
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails));
        }
    }
}
