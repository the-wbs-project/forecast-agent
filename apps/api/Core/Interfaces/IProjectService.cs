using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface IProjectService
{
    Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(Guid id);
    Task<ApiResponseDto<PagedResultDto<ProjectDto>>> GetProjectsAsync(
        int pageNumber, 
        int pageSize, 
        string? status = null, 
        Guid? organizationId = null, 
        string? search = null);
    Task<ApiResponseDto<ProjectDto>> CreateAsync(CreateProjectDto dto, Guid createdBy);
    Task<ApiResponseDto<ProjectDto>> UpdateAsync(UpdateProjectDto dto, Guid updatedBy);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, Guid deletedBy);
    Task<ApiResponseDto<bool>> ExistsAsync(Guid id);
    Task<ApiResponseDto<ProjectStatsDto>> GetProjectStatsAsync(Guid id);
}

public class ProjectStatsDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TotalSchedules { get; set; }
    public int TotalAnalyses { get; set; }
    public decimal CompletionPercentage { get; set; }
    public DateTime? EstimatedCompletionDate { get; set; }
}