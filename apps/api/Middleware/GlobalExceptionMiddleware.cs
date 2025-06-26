using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Api.Middleware;

/// <summary>
/// Global exception handling middleware
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ApiResponseDto<object>
        {
            Success = false,
            Data = null
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.Message = validationEx.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case UnauthorizedAccessException:
                response.Message = "Unauthorized access";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                break;

            case KeyNotFoundException:
                response.Message = "Resource not found";
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                break;

            case ArgumentException argEx:
                response.Message = argEx.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case InvalidOperationException invalidOpEx:
                response.Message = invalidOpEx.Message;
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            case TimeoutException:
                response.Message = "Request timeout";
                context.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                break;

            case NotSupportedException notSupportedEx:
                response.Message = $"Operation not supported: {notSupportedEx.Message}";
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                break;

            default:
                response.Message = _environment.IsDevelopment() 
                    ? exception.Message 
                    : "An internal server error occurred";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                break;
        }

        // Add detailed error information in development
        if (_environment.IsDevelopment())
        {
            response.Data = new
            {
                exception.Message,
                exception.StackTrace,
                InnerException = exception.InnerException?.Message
            };
        }

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}