namespace MyApp.Common
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public object? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        public ApiResponse() { }

        // Full constructor
        public ApiResponse(object? data, bool success = true, int statusCode = 200, string message = "")
        {
            Success = success;
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }

        // Quick constructor: only Data + Success
        public ApiResponse(object? data, bool success)
        {
            Data = data;
            Success = success;
            StatusCode = success ? 200 : 400;
        }

        // Convenience factory methods
        public static ApiResponse SuccessResponse(object? data, string message = "", int statusCode = 200)
        {
            return new ApiResponse(data, true, statusCode, message);
        }

        public static ApiResponse FailResponse(string message, int statusCode = 400, object? data = null)
        {
            return new ApiResponse(data, false, statusCode, message);
        }
    }
}
