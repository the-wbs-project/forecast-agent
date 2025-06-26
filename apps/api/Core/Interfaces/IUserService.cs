using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface IUserService
{
    Task<ApiResponseDto<UserDto>> GetUserByIdAsync(Guid id);
    Task<ApiResponseDto<PagedResultDto<UserDto>>> GetUsersAsync(
        int pageNumber, 
        int pageSize, 
        string? role = null, 
        bool? isActive = null, 
        Guid? organizationId = null, 
        string? search = null);
    Task<ApiResponseDto<UserDto>> CreateAsync(CreateUserDto dto, Guid createdBy);
    Task<ApiResponseDto<UserDto>> UpdateAsync(UpdateUserDto dto, Guid updatedBy);
    Task<ApiResponseDto<bool>> DeactivateAsync(Guid id, Guid deactivatedBy);
    Task<ApiResponseDto<bool>> ActivateAsync(Guid id, Guid activatedBy);
    Task<ApiResponseDto<bool>> ExistsAsync(Guid id);
    Task<ApiResponseDto<bool>> IsEmailAvailableAsync(string email);
    Task<ApiResponseDto<bool>> ChangePasswordAsync(Guid id, ChangePasswordDto dto, Guid changedBy);
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}