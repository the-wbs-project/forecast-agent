namespace WeatherGuard.Core.Entities;

public class BIMModelVersion : BaseEntity
{
    public Guid ModelId { get; set; }
    public int VersionNumber { get; set; }
    public string? VersionTag { get; set; }
    public string? Description { get; set; }
    public string? FileReference { get; set; }
    public long? FileSize { get; set; }
    public string? FileHash { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsActive { get; set; } = false;

    // Navigation properties
    public virtual BIMModel Model { get; set; } = null!;
    public virtual User Creator { get; set; } = null!;

    // Computed properties
    public string FileSizeFormatted => FileSize?.ToString("N0") + " bytes" ?? "0 bytes";
}