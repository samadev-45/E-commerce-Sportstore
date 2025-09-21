using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Common;


namespace MyApp.Helpers
{
    public static class ControllerExtensions
    {
        public static IActionResult OkResponse<T>(this ControllerBase controller, T data, string message = "Success")
        {
            var response = new ApiResponse<T>(data, true, 200, message);
            return controller.Ok(response);
        }

        public static IActionResult BadResponse(this ControllerBase controller, string message, int statusCode = 400)
        {
            var response = new ApiResponse<object>(null, false, statusCode, message);
            return controller.StatusCode(statusCode, response);
        }
    }
}
