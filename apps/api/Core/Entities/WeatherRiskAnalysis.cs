namespace WeatherGuard.Core.Entities;

public class WeatherRiskAnalysis : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTime AnalysisDate { get; set; }
    public string? WeatherDataSource { get; set; }
    public decimal? RiskScore { get; set; }
    public int? TotalDelayDays { get; set; }
    public decimal? TotalCostImpact { get; set; }
    public string? AnalysisResults { get; set; }
    public Guid GeneratedBy { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ProjectSchedule Schedule { get; set; } = null!;
    public virtual User GeneratedByUser { get; set; } = null!;
    public virtual ICollection<TaskRiskDetail> TaskRiskDetails { get; set; } = new List<TaskRiskDetail>();

    // Computed properties
    public string RiskScoreDescription => RiskScore switch
    {
        >= 8 => "Very High Risk",
        >= 6 => "High Risk",
        >= 4 => "Medium Risk",
        >= 2 => "Low Risk",
        _ => "Very Low Risk"
    };

    public bool HasSignificantRisk => RiskScore >= 6;
}