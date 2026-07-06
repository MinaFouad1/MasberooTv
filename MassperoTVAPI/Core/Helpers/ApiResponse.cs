namespace MassperoTVAPI.Core.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public int StatusCode { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Operation completed successfully.", int statusCode = 200)
        => new() { Success = true, Message = message, Data = data, StatusCode = statusCode };

    public static ApiResponse<T> CreatedResponse(T data, string message = "Resource created successfully.")
        => new() { Success = true, Message = message, Data = data, StatusCode = 201 };

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, List<string>? errors = null)
        => new() { Success = false, Message = message, StatusCode = statusCode, Errors = errors };

    public static ApiResponse<T> NotFoundResponse(string message = "Resource not found.")
        => new() { Success = false, Message = message, StatusCode = 404 };

    public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized access.")
        => new() { Success = false, Message = message, StatusCode = 401 };

    public static ApiResponse<T> ForbiddenResponse(string message = "Access forbidden.")
        => new() { Success = false, Message = message, StatusCode = 403 };
}
