using WeatherGuard.Api.Services;

namespace WeatherGuard.Api.Middleware;

public class DatadogLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly DatadogService _datadogService;
    private readonly ILogger<DatadogLoggingMiddleware> _logger;

    public DatadogLoggingMiddleware(RequestDelegate next, DatadogService datadogService, ILogger<DatadogLoggingMiddleware> logger)
    {
        _next = next;
        _datadogService = datadogService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        _datadogService.AddLog(
            "DatadogLoggingMiddleware",
            "Info",
            requestId,
            $"Request started: {context.Request.Method} {context.Request.Path}",
            new { 
                method = context.Request.Method,
                path = context.Request.Path.ToString(),
                userAgent = context.Request.Headers["User-Agent"].ToString()
            },
            null);

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _datadogService.AddLog(
                "DatadogLoggingMiddleware",
                "Error",
                requestId,
                $"Request failed: {context.Request.Method} {context.Request.Path}",
                new { 
                    method = context.Request.Method,
                    path = context.Request.Path.ToString(),
                    statusCode = context.Response.StatusCode,
                    duration = stopwatch.ElapsedMilliseconds
                },
                ex);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            _datadogService.AddLog(
                "DatadogLoggingMiddleware",
                "Info",
                requestId,
                $"Request completed: {context.Request.Method} {context.Request.Path} - {context.Response.StatusCode}",
                new { 
                    method = context.Request.Method,
                    path = context.Request.Path.ToString(),
                    statusCode = context.Response.StatusCode,
                    duration = stopwatch.ElapsedMilliseconds
                },
                null);
        }
    }
}