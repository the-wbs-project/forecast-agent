using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;

namespace WeatherGuard.Core.Interfaces;

public interface ITaskService
{
    Task<ApiResponseDto<TaskDto>> GetTaskByIdAsync(Guid id);
    Task<ApiResponseDto<PagedResultDto<TaskDto>>> GetTasksAsync(
        int pageNumber, 
        int pageSize, 
        Guid? projectId = null, 
        string? status = null, 
        string? priority = null, 
        Guid? assignedTo = null, 
        bool? isOverdue = null, 
        string? search = null);
    Task<ApiResponseDto<TaskDto>> CreateAsync(CreateTaskDto dto, Guid createdBy);
    Task<ApiResponseDto<TaskDto>> UpdateAsync(UpdateTaskDto dto, Guid updatedBy);
    Task<ApiResponseDto<bool>> DeleteAsync(Guid id, Guid deletedBy);
    Task<ApiResponseDto<TaskDto>> StartTaskAsync(Guid id, Guid startedBy);
    Task<ApiResponseDto<TaskDto>> CompleteTaskAsync(Guid id, Guid completedBy);
    Task<ApiResponseDto<TaskDto>> AssignTaskAsync(Guid id, Guid assignedTo, Guid assignedBy);
    Task<ApiResponseDto<IEnumerable<TaskDependencyDto>>> GetTaskDependenciesAsync(Guid id);
    Task<ApiResponseDto<bool>> AddDependencyAsync(Guid taskId, Guid dependsOnTaskId, Guid addedBy);
    Task<ApiResponseDto<bool>> RemoveDependencyAsync(Guid taskId, Guid dependencyId, Guid removedBy);
    Task<ApiResponseDto<IEnumerable<TaskHierarchyDto>>> GetTaskHierarchyAsync(Guid projectId);
    Task<ApiResponseDto<IEnumerable<TaskDto>>> GetWeatherSensitiveTasksAsync(Guid projectId);
}

public class TaskDependencyDto
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public Guid DependsOnTaskId { get; set; }
    public string DependsOnTaskName { get; set; } = string.Empty;
    public string DependencyType { get; set; } = "FinishToStart";
    public DateTime CreatedAt { get; set; }
}