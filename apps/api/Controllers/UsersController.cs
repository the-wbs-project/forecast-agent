using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for managing users
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Get all users with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="role">Filter by role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="organizationId">Filter by organization ID</param>
    /// <param name="search">Search term for user name or email</param>
    /// <returns>Paginated list of users</returns>
    [HttpGet]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            // Use organization from claims if not provided and user doesn't have admin rights
            organizationId ??= GetCurrentOrganizationId();

            var result = await _userService.GetUsersAsync(
                validPageNumber, 
                validPageSize, 
                role, 
                isActive, 
                organizationId, 
                search);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving users: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        try
        {
            var result = await _userService.GetUserByIdAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving user: {ex.Message}");
        }
    }

    /// <summary>
    /// Get current user profile
    /// </summary>
    /// <returns>Current user details</returns>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _userService.GetUserByIdAsync(currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving current user: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="createDto">User creation data</param>
    /// <returns>Created user</returns>
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid user data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            // Ensure user can only create users for their organization (unless admin)
            var currentOrgId = GetCurrentOrganizationId();
            if (currentOrgId.HasValue && createDto.OrganizationId != currentOrgId.Value)
                return ForbiddenResponse("Cannot create user for different organization");

            var result = await _userService.CreateAsync(createDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetUser), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error creating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="updateDto">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid user data");

            // Ensure the ID in the URL matches the DTO
            updateDto.Id = id;

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            // Users can update their own profile, or admins can update others
            if (id != currentUserId.Value)
            {
                // TODO: Add role-based authorization check for admin users
                // For now, allow all authenticated users to update others
            }

            var result = await _userService.UpdateAsync(updateDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Update current user profile
    /// </summary>
    /// <param name="updateDto">User update data</param>
    /// <returns>Updated user</returns>
    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserDto updateDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            updateDto.Id = currentUserId.Value;
            return await UpdateUser(currentUserId.Value, updateDto);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating current user: {ex.Message}");
        }
    }

    /// <summary>
    /// Deactivate a user (soft delete)
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _userService.DeactivateAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error deactivating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Activate a user
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _userService.ActivateAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error activating user: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a user exists
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>Boolean indicating existence</returns>
    [HttpHead("{id:guid}")]
    [HttpGet("{id:guid}/exists")]
    public async Task<IActionResult> UserExists(Guid id)
    {
        try
        {
            var result = await _userService.ExistsAsync(id);
            
            if (HttpContext.Request.Method == "HEAD")
            {
                return result.Data == true ? Ok() : NotFound();
            }
            
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error checking user existence: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if email is available
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <returns>Boolean indicating availability</returns>
    [HttpGet("email-available")]
    public async Task<IActionResult> IsEmailAvailable([FromQuery] string email)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email))
                return ValidationErrorResponse("Email is required");

            var result = await _userService.IsEmailAvailableAsync(email);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error checking email availability: {ex.Message}");
        }
    }

    /// <summary>
    /// Get users by organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="role">Filter by role</param>
    /// <param name="isActive">Filter by active status</param>
    /// <returns>Paginated list of users for the organization</returns>
    [HttpGet("organization/{organizationId:guid}")]
    public async Task<IActionResult> GetUsersByOrganization(
        Guid organizationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null)
    {
        try
        {
            // Ensure user can only access users from their organization (unless admin)
            var currentOrgId = GetCurrentOrganizationId();
            if (currentOrgId.HasValue && organizationId != currentOrgId.Value)
                return ForbiddenResponse("Cannot access users from different organization");

            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _userService.GetUsersAsync(
                validPageNumber, 
                validPageSize, 
                role, 
                isActive, 
                organizationId);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving organization users: {ex.Message}");
        }
    }

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="id">User ID</param>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Success status</returns>
    [HttpPatch("{id:guid}/password")]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid password data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            // Users can only change their own password unless they're admin
            if (id != currentUserId.Value)
            {
                // TODO: Add role-based authorization check for admin users
                return ForbiddenResponse("Cannot change password for another user");
            }

            var result = await _userService.ChangePasswordAsync(id, changePasswordDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error changing password: {ex.Message}");
        }
    }

    /// <summary>
    /// Change current user password
    /// </summary>
    /// <param name="changePasswordDto">Password change data</param>
    /// <returns>Success status</returns>
    [HttpPatch("me/password")]
    public async Task<IActionResult> ChangeCurrentUserPassword([FromBody] ChangePasswordDto changePasswordDto)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            return await ChangePassword(currentUserId.Value, changePasswordDto);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error changing current user password: {ex.Message}");
        }
    }
}