using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for managing projects
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : BaseController
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Get all projects with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="status">Filter by status</param>
    /// <param name="organizationId">Filter by organization ID</param>
    /// <param name="search">Search term for project name or description</param>
    /// <returns>Paginated list of projects</returns>
    [HttpGet]
    public async Task<IActionResult> GetProjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? organizationId = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            // Use organization from claims if not provided and user doesn't have admin rights
            organizationId ??= GetCurrentOrganizationId();

            var result = await _projectService.GetProjectsAsync(
                validPageNumber, 
                validPageSize, 
                status, 
                organizationId, 
                search);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving projects: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        try
        {
            var result = await _projectService.GetProjectByIdAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="createDto">Project creation data</param>
    /// <returns>Created project</returns>
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid project data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            // Ensure user can only create projects for their organization (unless admin)
            var currentOrgId = GetCurrentOrganizationId();
            if (currentOrgId.HasValue && createDto.OrganizationId != currentOrgId.Value)
                return ForbiddenResponse("Cannot create project for different organization");

            var result = await _projectService.CreateAsync(createDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetProject), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error creating project: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <param name="updateDto">Project update data</param>
    /// <returns>Updated project</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid project data");

            // Ensure the ID in the URL matches the DTO
            updateDto.Id = id;

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectService.UpdateAsync(updateDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating project: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _projectService.DeleteAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error deleting project: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if a project exists
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Boolean indicating existence</returns>
    [HttpHead("{id:guid}")]
    [HttpGet("{id:guid}/exists")]
    public async Task<IActionResult> ProjectExists(Guid id)
    {
        try
        {
            var result = await _projectService.ExistsAsync(id);
            
            if (HttpContext.Request.Method == "HEAD")
            {
                return result.Data == true ? Ok() : NotFound();
            }
            
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error checking project existence: {ex.Message}");
        }
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    /// <param name="id">Project ID</param>
    /// <returns>Project statistics</returns>
    [HttpGet("{id:guid}/stats")]
    public async Task<IActionResult> GetProjectStats(Guid id)
    {
        try
        {
            var result = await _projectService.GetProjectStatsAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project statistics: {ex.Message}");
        }
    }

    /// <summary>
    /// Get projects by organization
    /// </summary>
    /// <param name="organizationId">Organization ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated list of projects for the organization</returns>
    [HttpGet("organization/{organizationId:guid}")]
    public async Task<IActionResult> GetProjectsByOrganization(
        Guid organizationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Ensure user can only access projects from their organization (unless admin)
            var currentOrgId = GetCurrentOrganizationId();
            if (currentOrgId.HasValue && organizationId != currentOrgId.Value)
                return ForbiddenResponse("Cannot access projects from different organization");

            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _projectService.GetProjectsAsync(
                validPageNumber, 
                validPageSize, 
                organizationId: organizationId);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving organization projects: {ex.Message}");
        }
    }

    /// <summary>
    /// Search projects by criteria
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Search results</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProjects(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return ValidationErrorResponse("Search term is required");

            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            var currentOrgId = GetCurrentOrganizationId();
            
            var result = await _projectService.GetProjectsAsync(
                validPageNumber, 
                validPageSize, 
                search: searchTerm,
                organizationId: currentOrgId);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error searching projects: {ex.Message}");
        }
    }
}