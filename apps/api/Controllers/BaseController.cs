using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Returns a standardized success response
    /// </summary>
    protected IActionResult SuccessResponse<T>(T data, string? message = null)
    {
        return Ok(new ApiResponseDto<T>
        {
            Success = true,
            Data = data,
            Message = message
        });
    }

    /// <summary>
    /// Returns a standardized error response
    /// </summary>
    protected IActionResult ErrorResponse(string message, int statusCode = 400)
    {
        var response = new ApiResponseDto<object>
        {
            Success = false,
            Message = message,
            Data = null
        };

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Returns a standardized not found response
    /// </summary>
    protected IActionResult NotFoundResponse(string message = "Resource not found")
    {
        return ErrorResponse(message, 404);
    }

    /// <summary>
    /// Returns a standardized validation error response
    /// </summary>
    protected IActionResult ValidationErrorResponse(string message = "Validation failed")
    {
        return ErrorResponse(message, 422);
    }

    /// <summary>
    /// Returns a standardized unauthorized response
    /// </summary>
    protected IActionResult UnauthorizedResponse(string message = "Unauthorized access")
    {
        return ErrorResponse(message, 401);
    }

    /// <summary>
    /// Returns a standardized forbidden response
    /// </summary>
    protected IActionResult ForbiddenResponse(string message = "Access forbidden")
    {
        return ErrorResponse(message, 403);
    }

    /// <summary>
    /// Returns a standardized server error response
    /// </summary>
    protected IActionResult ServerErrorResponse(string message = "Internal server error")
    {
        return ErrorResponse(message, 500);
    }

    /// <summary>
    /// Handles service response and converts to appropriate HTTP response
    /// </summary>
    protected IActionResult HandleServiceResponse<T>(ApiResponseDto<T> serviceResponse)
    {
        if (serviceResponse.Success)
        {
            return Ok(serviceResponse);
        }

        // Determine appropriate status code based on message content
        var message = serviceResponse.Message ?? "Unknown error";
        
        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return NotFound(serviceResponse);
        
        if (message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            return Unauthorized(serviceResponse);
        
        if (message.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return StatusCode(403, serviceResponse);
        
        if (message.Contains("validation", StringComparison.OrdinalIgnoreCase) || 
            message.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            return StatusCode(422, serviceResponse);

        return BadRequest(serviceResponse);
    }

    /// <summary>
    /// Gets the current user ID from claims (placeholder for authentication)
    /// </summary>
    protected Guid? GetCurrentUserId()
    {
        // This would typically extract the user ID from JWT claims
        // For now, return a placeholder
        var userIdClaim = User?.FindFirst("sub")?.Value ?? User?.FindFirst("userId")?.Value;
        
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
            
        return null;
    }

    /// <summary>
    /// Gets the current organization ID from claims (placeholder for authentication)
    /// </summary>
    protected Guid? GetCurrentOrganizationId()
    {
        // This would typically extract the organization ID from JWT claims
        var orgIdClaim = User?.FindFirst("organizationId")?.Value;
        
        if (Guid.TryParse(orgIdClaim, out var orgId))
            return orgId;
            
        return null;
    }

    /// <summary>
    /// Validates pagination parameters
    /// </summary>
    protected (int pageNumber, int pageSize) ValidatePaginationParameters(int pageNumber = 1, int pageSize = 10)
    {
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Max(1, Math.Min(100, pageSize)); // Limit to max 100 items per page
        
        return (pageNumber, pageSize);
    }
}