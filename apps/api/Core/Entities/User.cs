namespace WeatherGuard.Core.Entities;

public class User : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = "User";
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Firm Organization { get; set; } = null!;
    public virtual ICollection<Project> CreatedProjects { get; set; } = new List<Project>();
    public virtual ICollection<ProjectSchedule> UploadedSchedules { get; set; } = new List<ProjectSchedule>();
    public virtual ICollection<WeatherRiskAnalysis> GeneratedAnalyses { get; set; } = new List<WeatherRiskAnalysis>();
    public virtual ICollection<BIMModel> CreatedBIMModels { get; set; } = new List<BIMModel>();
    public virtual ICollection<BIMCoordinationIssue> AssignedIssues { get; set; } = new List<BIMCoordinationIssue>();
    public virtual ICollection<BIMCoordinationIssue> CreatedIssues { get; set; } = new List<BIMCoordinationIssue>();
    public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
}