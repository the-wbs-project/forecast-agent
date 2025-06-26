using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface IWeatherRiskAnalysisService
{
    Task<ApiResponseDto<WeatherRiskAnalysisDto>> GetAnalysisByIdAsync(Guid id);
    Task<ApiResponseDto<PagedResultDto<WeatherRiskAnalysisDto>>> GetAnalysesAsync(
        int pageNumber, 
        int pageSize, 
        Guid? projectId = null, 
        string? riskLevel = null, 
        string? weatherCondition = null, 
        string? search = null);
    Task<ApiResponseDto<WeatherRiskAnalysisDto>> CreateAsync(CreateWeatherRiskAnalysisDto dto, Guid createdBy);
    Task<ApiResponseDto<WeatherRiskAnalysisDto>> UpdateAsync(UpdateWeatherRiskAnalysisDto dto, Guid updatedBy);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, Guid deletedBy);
    Task<ApiResponseDto<WeatherRiskAnalysisDto>> GenerateAnalysisAsync(Guid projectId, GenerateAnalysisDto dto, Guid generatedBy);
    Task<ApiResponseDto<PagedResultDto<WeatherRiskAnalysisDto>>> GetHighRiskAnalysesAsync(
        int pageNumber, 
        int pageSize, 
        Guid? organizationId = null);
    Task<ApiResponseDto<ProjectRiskSummaryDto>> GetProjectRiskSummaryAsync(Guid projectId);
    Task<ApiResponseDto<IEnumerable<WeatherForecastDto>>> GetWeatherForecastAsync(decimal latitude, decimal longitude, int days);
}

public class ProjectRiskSummaryDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int TotalAnalyses { get; set; }
    public int HighRiskAnalyses { get; set; }
    public int MediumRiskAnalyses { get; set; }
    public int LowRiskAnalyses { get; set; }
    public decimal OverallRiskScore { get; set; }
    public string OverallRiskLevel { get; set; } = string.Empty;
    public DateTime? NextHighRiskDate { get; set; }
    public List<string> MostCommonRisks { get; set; } = new();
    public List<WeatherRiskAnalysisDto> RecentAnalyses { get; set; } = new();
}

public class GenerateAnalysisDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<string> WeatherConditions { get; set; } = new();
    public bool IncludeTaskRiskDetails { get; set; } = true;
}

public class WeatherForecastDto
{
    public DateTime Date { get; set; }
    public decimal Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public decimal Humidity { get; set; }
    public decimal WindSpeed { get; set; }
    public decimal PrecipitationChance { get; set; }
}