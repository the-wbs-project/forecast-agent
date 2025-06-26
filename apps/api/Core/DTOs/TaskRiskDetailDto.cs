using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class TaskRiskDetailDto : BaseDto
{
    public Guid AnalysisId { get; set; }
    public Guid TaskId { get; set; }
    public string? RiskType { get; set; }
    public decimal? Probability { get; set; }
    public int? ImpactDays { get; set; }
    public decimal? ImpactCost { get; set; }
    public string? MitigationSuggestions { get; set; }
    public string? WeatherForecast { get; set; }
    
    // Computed properties
    public string ProbabilityDescription { get; set; } = string.Empty;
    public decimal? ExpectedImpactCost { get; set; }
    
    // Navigation properties
    public string? TaskName { get; set; }
    public DateTime? TaskStartDate { get; set; }
    public DateTime? TaskEndDate { get; set; }
}

public class CreateTaskRiskDetailDto : BaseCreateDto
{
    [Required]
    public Guid AnalysisId { get; set; }

    [Required]
    public Guid TaskId { get; set; }

    [StringLength(100)]
    public string? RiskType { get; set; }

    [Range(0, 1)]
    public decimal? Probability { get; set; }

    [Range(0, int.MaxValue)]
    public int? ImpactDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ImpactCost { get; set; }

    [StringLength(2000)]
    public string? MitigationSuggestions { get; set; }

    [StringLength(1000)]
    public string? WeatherForecast { get; set; }
}

public class UpdateTaskRiskDetailDto : BaseUpdateDto
{
    [StringLength(100)]
    public string? RiskType { get; set; }

    [Range(0, 1)]
    public decimal? Probability { get; set; }

    [Range(0, int.MaxValue)]
    public int? ImpactDays { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ImpactCost { get; set; }

    [StringLength(2000)]
    public string? MitigationSuggestions { get; set; }

    [StringLength(1000)]
    public string? WeatherForecast { get; set; }
}