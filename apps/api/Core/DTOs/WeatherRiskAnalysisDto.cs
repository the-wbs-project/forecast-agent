using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class WeatherRiskAnalysisDto : BaseDto
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
    
    // Computed properties
    public string RiskScoreDescription { get; set; } = string.Empty;
    public bool HasSignificantRisk { get; set; }
    
    // Navigation properties
    public string? ProjectName { get; set; }
    public string? ScheduleFileName { get; set; }
    public string? GeneratedByName { get; set; }
    public int TaskRiskDetailsCount { get; set; }
    public List<TaskRiskDetailDto> TaskRiskDetails { get; set; } = new();
}

public class CreateWeatherRiskAnalysisDto : BaseCreateDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid ScheduleId { get; set; }

    [StringLength(100)]
    public string? WeatherDataSource { get; set; }

    [Range(0, 10)]
    public decimal? RiskScore { get; set; }

    [Range(0, int.MaxValue)]
    public int? TotalDelayDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TotalCostImpact { get; set; }

    public string? AnalysisResults { get; set; }
}

public class UpdateWeatherRiskAnalysisDto : BaseUpdateDto
{
    [StringLength(100)]
    public string? WeatherDataSource { get; set; }

    [Range(0, 10)]
    public decimal? RiskScore { get; set; }

    [Range(0, int.MaxValue)]
    public int? TotalDelayDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? TotalCostImpact { get; set; }

    public string? AnalysisResults { get; set; }
}

public class WeatherRiskAnalysisSummaryDto
{
    public Guid Id { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    public decimal? RiskScore { get; set; }
    public string RiskScoreDescription { get; set; } = string.Empty;
    public int? TotalDelayDays { get; set; }
    public decimal? TotalCostImpact { get; set; }
    public int AffectedTasksCount { get; set; }
}