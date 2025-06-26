using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for authentication and authorization
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="loginDto">Login credentials</param>
    /// <returns>JWT token and user information</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid login credentials");

            var result = await _authService.LoginAsync(loginDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error during login: {ex.Message}");
        }
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="registerDto">Registration data</param>
    /// <returns>Created user and JWT token</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid registration data");

            var result = await _authService.RegisterAsync(registerDto);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(Login), 
                    new { }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error during registration: {ex.Message}");
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="refreshDto">Refresh token data</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid refresh token");

            var result = await _authService.RefreshTokenAsync(refreshDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error refreshing token: {ex.Message}");
        }
    }

    /// <summary>
    /// Logout user (invalidate token)
    /// </summary>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _authService.LogoutAsync(currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error during logout: {ex.Message}");
        }
    }

    /// <summary>
    /// Request password reset
    /// </summary>
    /// <param name="resetRequestDto">Password reset request data</param>
    /// <returns>Success status</returns>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto resetRequestDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid email address");

            var result = await _authService.ForgotPasswordAsync(resetRequestDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error processing password reset request: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset password with token
    /// </summary>
    /// <param name="resetDto">Password reset data</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid password reset data");

            var result = await _authService.ResetPasswordAsync(resetDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error resetting password: {ex.Message}");
        }
    }

    /// <summary>
    /// Verify email address
    /// </summary>
    /// <param name="verifyDto">Email verification data</param>
    /// <returns>Success status</returns>
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid verification data");

            var result = await _authService.VerifyEmailAsync(verifyDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error verifying email: {ex.Message}");
        }
    }

    /// <summary>
    /// Resend email verification
    /// </summary>
    /// <param name="resendDto">Resend verification data</param>
    /// <returns>Success status</returns>
    [HttpPost("resend-verification")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto resendDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid email address");

            var result = await _authService.ResendVerificationAsync(resendDto);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error resending verification: {ex.Message}");
        }
    }
}