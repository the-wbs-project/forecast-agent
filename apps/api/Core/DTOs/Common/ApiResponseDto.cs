namespace WeatherGuard.Core.DTOs.Common;

public class ApiResponseDto<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponseDto<T> SuccessResult(T data, string? message = null)
    {
        return new ApiResponseDto<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponseDto<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponseDto<T> ErrorResult(List<string> errors)
    {
        return new ApiResponseDto<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}

public class ApiResponseDto : ApiResponseDto<object>
{
    public static ApiResponseDto SuccessResult(string? message = null)
    {
        return new ApiResponseDto
        {
            Success = true,
            Message = message
        };
    }

    public new static ApiResponseDto ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponseDto
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public new static ApiResponseDto ErrorResult(List<string> errors)
    {
        return new ApiResponseDto
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}