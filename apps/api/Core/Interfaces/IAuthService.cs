using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface IAuthService
{
    Task<ApiResponseDto<LoginResponseDto>> LoginAsync(LoginDto loginDto);
    Task<ApiResponseDto<LoginResponseDto>> RegisterAsync(RegisterDto registerDto);
    Task<ApiResponseDto<LoginResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshDto);
    Task<ApiResponseDto<bool>> LogoutAsync(Guid userId);
    Task<ApiResponseDto<bool>> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<ApiResponseDto<bool>> ResetPasswordAsync(ResetPasswordDto resetDto);
    Task<ApiResponseDto<bool>> VerifyEmailAsync(VerifyEmailDto verifyDto);
    Task<ApiResponseDto<bool>> ResendVerificationAsync(ResendVerificationDto resendDto);
}

public class RegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
}

public class RefreshTokenDto
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailDto
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationDto
{
    public string Email { get; set; } = string.Empty;
}