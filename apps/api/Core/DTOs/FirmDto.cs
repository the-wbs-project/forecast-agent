using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class FirmDto : BaseDto
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class CreateFirmDto : BaseCreateDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z0-9-]+$", ErrorMessage = "Subdomain can only contain letters, numbers, and hyphens")]
    public string Subdomain { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

public class UpdateFirmDto : BaseUpdateDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}