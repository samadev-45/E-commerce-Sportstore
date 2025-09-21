using Microsoft.AspNetCore.Http;
using MyApp.DTOs;
using MyApp.Common;

namespace MyApp.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new ApiResponse<string>
            {
                Success = false,
                StatusCode = 500,
                Data = null,
                Message = ex.Message // or a generic message like "Internal server error"
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
