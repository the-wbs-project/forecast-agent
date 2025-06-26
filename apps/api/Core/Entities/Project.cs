namespace WeatherGuard.Core.Entities;

public class Project : BaseEntity
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
    public string Status { get; set; } = "Active";

    // BIM-specific fields
    public bool BIMEnabled { get; set; } = false;
    public string? CoordinateSystemId { get; set; }
    public decimal? NorthDirection { get; set; }
    public decimal? BuildingHeight { get; set; }
    public decimal? GrossFloorArea { get; set; }
    public int? NumberOfStoreys { get; set; }

    // Navigation properties
    public virtual Firm Organization { get; set; } = null!;
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<ProjectSchedule> Schedules { get; set; } = new List<ProjectSchedule>();
    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    public virtual ICollection<WeatherRiskAnalysis> WeatherRiskAnalyses { get; set; } = new List<WeatherRiskAnalysis>();
    public virtual ICollection<BIMModel> BIMModels { get; set; } = new List<BIMModel>();
    public virtual ICollection<BIMCoordinationIssue> CoordinationIssues { get; set; } = new List<BIMCoordinationIssue>();

    // Computed properties
    public int? DurationInDays => StartDate.HasValue && EndDate.HasValue 
        ? EndDate.Value.DayNumber - StartDate.Value.DayNumber 
        : null;
}