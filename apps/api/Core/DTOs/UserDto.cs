using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class UserDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public string FullName { get; set; } = string.Empty;
    
    // Navigation properties (optional, for detailed responses)
    public string? OrganizationName { get; set; }
}

public class CreateUserDto : BaseCreateDto
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "User";

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateUserDto : BaseUpdateDto
{
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [StringLength(100)]
    public string? FirstName { get; set; }

    [StringLength(100)]
    public string? LastName { get; set; }

    [StringLength(50)]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}