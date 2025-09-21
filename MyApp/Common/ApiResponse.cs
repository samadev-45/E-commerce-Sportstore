namespace MyApp.Common
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        public ApiResponse() { }

        public ApiResponse(T? data, bool success = true, int statusCode = 200, string message = "")
        {
            Success = success;
            StatusCode = statusCode;
            Data = data;
            Message = message;
        }
    }
}
