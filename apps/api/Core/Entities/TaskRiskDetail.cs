namespace WeatherGuard.Core.Entities;

public class TaskRiskDetail : BaseEntity
{
    public Guid AnalysisId { get; set; }
    public Guid TaskId { get; set; }
    public string? RiskType { get; set; }
    public decimal? Probability { get; set; }
    public int? ImpactDays { get; set; }
    public decimal? ImpactCost { get; set; }
    public string? MitigationSuggestions { get; set; }
    public string? WeatherForecast { get; set; }

    // Navigation properties
    public virtual WeatherRiskAnalysis Analysis { get; set; } = null!;
    public virtual Task Task { get; set; } = null!;

    // Computed properties
    public string ProbabilityDescription => Probability switch
    {
        >= 0.8m => "Very High",
        >= 0.6m => "High",
        >= 0.4m => "Medium",
        >= 0.2m => "Low",
        _ => "Very Low"
    };

    public decimal? ExpectedImpactCost => Probability.HasValue && ImpactCost.HasValue 
        ? Probability.Value * ImpactCost.Value 
        : null;
}