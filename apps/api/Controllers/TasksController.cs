using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for managing tasks
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TasksController : BaseController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    /// <summary>
    /// Get all tasks with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <param name="assignedTo">Filter by assigned user</param>
    /// <param name="isOverdue">Filter by overdue status</param>
    /// <param name="search">Search term for task name or description</param>
    /// <returns>Paginated list of tasks</returns>
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null,
        [FromQuery] Guid? assignedTo = null,
        [FromQuery] bool? isOverdue = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _taskService.GetTasksAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                status, 
                priority, 
                assignedTo, 
                isOverdue, 
                search);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        try
        {
            var result = await _taskService.GetTaskByIdAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving task: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new task
    /// </summary>
    /// <param name="createDto">Task creation data</param>
    /// <returns>Created task</returns>
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid task data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.CreateAsync(createDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetTask), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error creating task: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateDto">Task update data</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid task data");

            // Ensure the ID in the URL matches the DTO
            updateDto.Id = id;

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.UpdateAsync(updateDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating task: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.DeleteAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error deleting task: {ex.Message}");
        }
    }

    /// <summary>
    /// Start a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> StartTask(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.StartTaskAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error starting task: {ex.Message}");
        }
    }

    /// <summary>
    /// Complete a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteTask(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.CompleteTaskAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error completing task: {ex.Message}");
        }
    }

    /// <summary>
    /// Assign a task to a user
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="assignDto">Assignment data</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}/assign")]
    public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskDto assignDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid assignment data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.AssignTaskAsync(id, assignDto.AssignedTo, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error assigning task: {ex.Message}");
        }
    }

    /// <summary>
    /// Get tasks by project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <returns>Paginated list of tasks for the project</returns>
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetTasksByProject(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _taskService.GetTasksAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                status, 
                priority);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Get tasks assigned to current user
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="status">Filter by status</param>
    /// <param name="priority">Filter by priority</param>
    /// <returns>Paginated list of assigned tasks</returns>
    [HttpGet("my-tasks")]
    public async Task<IActionResult> GetMyTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? priority = null)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _taskService.GetTasksAsync(
                validPageNumber, 
                validPageSize, 
                assignedTo: currentUserId.Value,
                status: status,
                priority: priority);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving assigned tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Get overdue tasks
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <returns>Paginated list of overdue tasks</returns>
    [HttpGet("overdue")]
    public async Task<IActionResult> GetOverdueTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _taskService.GetTasksAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                isOverdue: true);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving overdue tasks: {ex.Message}");
        }
    }

    /// <summary>
    /// Get task dependencies
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>List of task dependencies</returns>
    [HttpGet("{id:guid}/dependencies")]
    public async Task<IActionResult> GetTaskDependencies(Guid id)
    {
        try
        {
            var result = await _taskService.GetTaskDependenciesAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving task dependencies: {ex.Message}");
        }
    }

    /// <summary>
    /// Add task dependency
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="dependencyDto">Dependency data</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/dependencies")]
    public async Task<IActionResult> AddTaskDependency(Guid id, [FromBody] AddTaskDependencyDto dependencyDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid dependency data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.AddDependencyAsync(id, dependencyDto.DependsOnTaskId, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error adding task dependency: {ex.Message}");
        }
    }

    /// <summary>
    /// Remove task dependency
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="dependencyId">Dependency ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}/dependencies/{dependencyId:guid}")]
    public async Task<IActionResult> RemoveTaskDependency(Guid id, Guid dependencyId)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _taskService.RemoveDependencyAsync(id, dependencyId, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error removing task dependency: {ex.Message}");
        }
    }
}

public class AssignTaskDto
{
    public Guid AssignedTo { get; set; }
}

public class AddTaskDependencyDto
{
    public Guid DependsOnTaskId { get; set; }
}