using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class ProjectScheduleDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    
    // Computed properties
    public bool IsProcessed { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    
    // Navigation properties
    public string? ProjectName { get; set; }
    public string? UploadedByName { get; set; }
    public int TaskCount { get; set; }
    public int AnalysisCount { get; set; }
}

public class CreateProjectScheduleDto : BaseCreateDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    [StringLength(255)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string FileType { get; set; } = string.Empty;

    [Required]
    public byte[] FileContent { get; set; } = Array.Empty<byte>();
}

public class UpdateProjectScheduleDto : BaseUpdateDto
{
    [StringLength(255)]
    public string? FileName { get; set; }

    [StringLength(50)]
    public string? Status { get; set; }

    public string? ErrorMessage { get; set; }
    
    public string? ParsedData { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
}

public class ScheduleUploadResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string FileSizeFormatted { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int ParsedTaskCount { get; set; }
}