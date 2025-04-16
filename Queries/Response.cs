using System;
using System.Net;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T Data { get; set; }
    public string ErrorCode { get; set; }
    public DateTime Timestamp { get; set; }

    // Constructor cho response thành công
    public ApiResponse(T data, string message = "Operation completed successfully")
    {
        Success = true;
        Message = message;
        StatusCode = HttpStatusCode.OK;
        Data = data;
        Timestamp = DateTime.UtcNow;
    }

    // Constructor cho response lỗi
    public ApiResponse(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string errorCode = null)
    {
        Success = false;
        Message = message;
        StatusCode = statusCode;
        ErrorCode = errorCode;
        Timestamp = DateTime.UtcNow;
    }

    // Factory method để tạo response thành công
    public static ApiResponse<T> CreateSuccess(T data, string message = "Operation completed successfully")
    {
        return new ApiResponse<T>(data, message);
    }

    // Factory method để tạo response lỗi
    public static ApiResponse<T> CreateError(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest, string errorCode = null)
    {
        return new ApiResponse<T>(message, statusCode, errorCode);
    }
}