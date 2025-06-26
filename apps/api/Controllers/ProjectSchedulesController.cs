using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;
using static WeatherGuard.Core.Interfaces.IProjectScheduleService;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for managing project schedules
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectSchedulesController : BaseController
{
    private readonly IProjectScheduleService _projectScheduleService;

    public ProjectSchedulesController(IProjectScheduleService projectScheduleService)
    {
        _projectScheduleService = projectScheduleService;
    }

    /// <summary>
    /// Get all project schedules with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="search">Search term for schedule name</param>
    /// <returns>Paginated list of project schedules</returns>
    [HttpGet]
    public async Task<IActionResult> GetProjectSchedules(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _projectScheduleService.GetSchedulesAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                search);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project schedules: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific project schedule by ID
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <returns>Project schedule details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProjectSchedule(Guid id)
    {
        try
        {
            var result = await _projectScheduleService.GetScheduleByIdAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new project schedule
    /// </summary>
    /// <param name="createDto">Schedule creation data</param>
    /// <returns>Created project schedule</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProjectSchedule([FromBody] CreateProjectScheduleDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid project schedule data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectScheduleService.CreateAsync(createDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetProjectSchedule), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error creating project schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing project schedule
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <param name="updateDto">Schedule update data</param>
    /// <returns>Updated project schedule</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProjectSchedule(Guid id, [FromBody] UpdateProjectScheduleDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid project schedule data");

            // Ensure the ID in the URL matches the DTO
            updateDto.Id = id;

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectScheduleService.UpdateAsync(updateDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating project schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a project schedule
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProjectSchedule(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectScheduleService.DeleteAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error deleting project schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Get schedules by project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of schedules for the project</returns>
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetSchedulesByProject(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _projectScheduleService.GetSchedulesAsync(
                validPageNumber, 
                validPageSize, 
                projectId);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project schedules: {ex.Message}");
        }
    }

    /// <summary>
    /// Upload schedule file (e.g., MS Project, Primavera)
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="file">Schedule file</param>
    /// <returns>Imported schedule</returns>
    [HttpPost("project/{projectId:guid}/upload")]
    public async Task<IActionResult> UploadScheduleFile(Guid projectId, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return ValidationErrorResponse("No file provided");

            var allowedExtensions = new[] { ".mpp", ".xml", ".xer", ".csv" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                return ValidationErrorResponse($"Unsupported file type. Allowed types: {string.Join(", ", allowedExtensions)}");

            if (file.Length > 50 * 1024 * 1024) // 50MB limit
                return ValidationErrorResponse("File size cannot exceed 50MB");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectScheduleService.ImportScheduleAsync(projectId, file, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetProjectSchedule), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error uploading schedule file: {ex.Message}");
        }
    }

    /// <summary>
    /// Export schedule to file format
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <param name="format">Export format (xml, csv, json)</param>
    /// <returns>Exported file</returns>
    [HttpGet("{id:guid}/export")]
    public async Task<IActionResult> ExportSchedule(Guid id, [FromQuery] string format = "xml")
    {
        try
        {
            var allowedFormats = new[] { "xml", "csv", "json" };
            if (!allowedFormats.Contains(format.ToLowerInvariant()))
                return ValidationErrorResponse($"Unsupported export format. Allowed formats: {string.Join(", ", allowedFormats)}");

            var result = await _projectScheduleService.ExportScheduleAsync(id, format);
            
            if (result.Success && result.Data != null)
            {
                var contentType = format.ToLowerInvariant() switch
                {
                    "xml" => "application/xml",
                    "csv" => "text/csv",
                    "json" => "application/json",
                    _ => "application/octet-stream"
                };

                var fileName = $"schedule_{id}_{DateTime.UtcNow:yyyyMMdd}.{format}";
                
                return File(result.Data.Content, contentType, fileName);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error exporting schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Get schedule statistics
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <returns>Schedule statistics</returns>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetScheduleStats(Guid id)
    {
        try
        {
            var result = await _projectScheduleService.GetScheduleStatsAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving schedule statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Get critical path for schedule
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <returns>Critical path tasks</returns>
    [HttpGet("{id:guid}/critical-path")]
    public async Task<IActionResult> GetCriticalPath(Guid id)
    {
        try
        {
            var result = await _projectScheduleService.GetCriticalPathAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving critical path: {ex.Message}");
        }
    }

    /// <summary>
    /// Validate schedule integrity
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <returns>Validation results</returns>
    [HttpPost("{id:guid}/validate")]
    public async Task<IActionResult> ValidateSchedule(Guid id)
    {
        try
        {
            var result = await _projectScheduleService.ValidateScheduleAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error validating schedule: {ex.Message}");
        }
    }

    /// <summary>
    /// Optimize schedule based on weather risks
    /// </summary>
    /// <param name="id">Schedule ID</param>
    /// <param name="optimizeDto">Optimization parameters</param>
    /// <returns>Optimized schedule</returns>
    [HttpPost("{id:guid}/optimize")]
    public async Task<IActionResult> OptimizeSchedule(Guid id, [FromBody] OptimizeScheduleDto optimizeDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid optimization parameters");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectScheduleService.OptimizeScheduleAsync(id, optimizeDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error optimizing schedule: {ex.Message}");
        }
    }
}

