namespace WeatherGuard.Core.Entities;

public class ProjectSchedule : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public byte[]? FileContent { get; set; }
    public string? ParsedData { get; set; }
    public Guid UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User UploadedByUser { get; set; } = null!;
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    public virtual ICollection<WeatherRiskAnalysis> WeatherRiskAnalyses { get; set; } = new List<WeatherRiskAnalysis>();

    // Computed properties
    public bool IsProcessed => ProcessedAt.HasValue;
    public string FileSizeFormatted => FileContent?.Length.ToString("N0") + " bytes" ?? "0 bytes";
}