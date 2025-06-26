using Microsoft.AspNetCore.Http;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface IProjectScheduleService
{
    Task<ApiResponseDto<ProjectScheduleDto>> GetScheduleByIdAsync(Guid id);
    Task<ApiResponseDto<PagedResultDto<ProjectScheduleDto>>> GetSchedulesAsync(
        int pageNumber, 
        int pageSize, 
        Guid? projectId = null, 
        string? search = null);
    Task<ApiResponseDto<ProjectScheduleDto>> CreateAsync(CreateProjectScheduleDto dto, Guid createdBy);
    Task<ApiResponseDto<ProjectScheduleDto>> UpdateAsync(UpdateProjectScheduleDto dto, Guid updatedBy);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, Guid deletedBy);
    Task<ApiResponseDto<ProjectScheduleDto>> ImportScheduleAsync(Guid projectId, IFormFile file, Guid uploadedBy);
    Task<ApiResponseDto<ScheduleExportDto>> ExportScheduleAsync(Guid id, string format);
    Task<ApiResponseDto<ScheduleStatsDto>> GetScheduleStatsAsync(Guid id);
    Task<ApiResponseDto<IEnumerable<TaskDto>>> GetCriticalPathAsync(Guid id);
    Task<ApiResponseDto<ScheduleValidationDto>> ValidateScheduleAsync(Guid id);
    Task<ApiResponseDto<ProjectScheduleDto>> OptimizeScheduleAsync(Guid id, OptimizeScheduleDto dto, Guid optimizedBy);
}

public class ScheduleValidationDto
{
    public bool IsValid { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public int TotalIssues => Issues.Count;
    public int CriticalIssues => Issues.Count(i => i.Severity == "Critical");
    public int WarningIssues => Issues.Count(i => i.Severity == "Warning");
    public int InfoIssues => Issues.Count(i => i.Severity == "Info");
}

public class ValidationIssue
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Critical, Warning, Info
    public Guid? TaskId { get; set; }
    public string? TaskName { get; set; }
    public string? Recommendation { get; set; }
}

public class OptimizeScheduleDto
{
    public bool ConsiderWeatherRisks { get; set; } = true;
    public bool ConsiderResourceAvailability { get; set; } = true;
    public bool MinimizeProjectDuration { get; set; } = true;
    public decimal WeatherRiskWeight { get; set; } = 0.3m;
    public decimal ResourceWeight { get; set; } = 0.4m;
    public decimal DurationWeight { get; set; } = 0.3m;
    public List<string> WeatherConditionsToAvoid { get; set; } = new();
}

public class ScheduleExportDto
{
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class ScheduleStatsDto
{
    public Guid ScheduleId { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int CriticalPathTasks { get; set; }
    public DateTime? EarliestStartDate { get; set; }
    public DateTime? LatestFinishDate { get; set; }
    public int TotalDurationDays { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int ResourceCount { get; set; }
    public int MilestoneCount { get; set; }
}