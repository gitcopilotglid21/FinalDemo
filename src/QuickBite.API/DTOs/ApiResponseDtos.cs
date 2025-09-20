namespace QuickBite.API.DTOs
{
    /// <summary>
    /// Standard API response wrapper
    /// </summary>
    /// <typeparam name="T">Type of data being returned</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// The response data
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Success message
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Creates a successful response
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T data, string? message = null)
        {
            return new ApiResponse<T>
            {
                Data = data,
                Message = message,
                Success = true
            };
        }

        /// <summary>
        /// Creates a success response without data
        /// </summary>
        public static ApiResponse<object> SuccessResponse(string message)
        {
            return new ApiResponse<object>
            {
                Message = message,
                Success = true
            };
        }
    }

    /// <summary>
    /// Error response structure
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Error details
        /// </summary>
        public ErrorDetails Error { get; set; } = new();
    }

    /// <summary>
    /// Error details structure
    /// </summary>
    public class ErrorDetails
    {
        /// <summary>
        /// Error code
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Human readable error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Additional error details
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Timestamp when the error occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}