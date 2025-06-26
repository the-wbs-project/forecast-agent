namespace WeatherGuard.Core.Entities;

public class BIMClashDetection : BaseEntity
{
    public Guid ModelId { get; set; }
    public Guid Element1Id { get; set; }
    public Guid Element2Id { get; set; }
    public string? ClashType { get; set; }
    public string? Severity { get; set; }
    public string? Status { get; set; }
    public string? Location { get; set; }  // Store as WKT string for now
    public decimal? VolumeIntersection { get; set; }
    public string? Description { get; set; }
    public string? Resolution { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime DetectedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual BIMModel Model { get; set; } = null!;
    public virtual BIMElement Element1 { get; set; } = null!;
    public virtual BIMElement Element2 { get; set; } = null!;
    public virtual User? AssignedUser { get; set; }

    // Computed properties
    public bool IsResolved => ResolvedAt.HasValue;
    public int DaysToResolve => IsResolved ? (int)(ResolvedAt!.Value - DetectedAt).TotalDays : 0;
}