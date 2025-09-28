using Microsoft.AspNetCore.Mvc;
using MyApp.DTOs;
using MyApp.Common;


namespace MyApp.Helpers
{
    public static class ControllerExtensions
    {
        public static IActionResult OkResponse(this ControllerBase controller, object? data, string message = "Success")
        {
            var response = ApiResponse.SuccessResponse(data, message);
            return controller.Ok(response);
        }

        public static IActionResult BadResponse(this ControllerBase controller, string message, int statusCode = 400)
        {
            var response = ApiResponse.FailResponse(message, statusCode);
            return controller.StatusCode(statusCode, response);
        }
    }
}
