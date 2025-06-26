using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class ProjectDto : BaseDto
{
    public Guid OrganizationId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid CreatedBy { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool BIMEnabled { get; set; }
    public string? CoordinateSystemId { get; set; }
    public decimal? NorthDirection { get; set; }
    public decimal? BuildingHeight { get; set; }
    public decimal? GrossFloorArea { get; set; }
    public int? NumberOfStoreys { get; set; }
    public int? DurationInDays { get; set; }
    
    // Navigation properties
    public string? OrganizationName { get; set; }
    public string? CreatorName { get; set; }
    public int ScheduleCount { get; set; }
    public int TaskCount { get; set; }
    public int AnalysisCount { get; set; }
}

public class CreateProjectDto : BaseCreateDto
{
    [Required]
    public Guid OrganizationId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }

    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    public decimal? Longitude { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(50)]
    public string Status { get; set; } = "Active";

    public bool BIMEnabled { get; set; } = false;

    [StringLength(50)]
    public string? CoordinateSystemId { get; set; }

    [Range(0, 360)]
    public decimal? NorthDirection { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? BuildingHeight { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? GrossFloorArea { get; set; }

    [Range(1, int.MaxValue)]
    public int? NumberOfStoreys { get; set; }
}

public class UpdateProjectDto : BaseUpdateDto
{
    [StringLength(255)]
    public string? Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }

    [Range(-90, 90)]
    public decimal? Latitude { get; set; }

    [Range(-180, 180)]
    public decimal? Longitude { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public bool? BIMEnabled { get; set; }

    [StringLength(50)]
    public string? CoordinateSystemId { get; set; }

    [Range(0, 360)]
    public decimal? NorthDirection { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? BuildingHeight { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? GrossFloorArea { get; set; }

    [Range(1, int.MaxValue)]
    public int? NumberOfStoreys { get; set; }
}