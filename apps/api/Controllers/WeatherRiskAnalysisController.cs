using Microsoft.AspNetCore.Mvc;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Interfaces;
using static WeatherGuard.Core.Interfaces.IWeatherRiskAnalysisService;

namespace WeatherGuard.Api.Controllers;

/// <summary>
/// Controller for managing weather risk analyses
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WeatherRiskAnalysisController : BaseController
{
    private readonly IWeatherRiskAnalysisService _weatherRiskAnalysisService;

    public WeatherRiskAnalysisController(IWeatherRiskAnalysisService weatherRiskAnalysisService)
    {
        _weatherRiskAnalysisService = weatherRiskAnalysisService;
    }

    /// <summary>
    /// Get all weather risk analyses with pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="riskLevel">Filter by risk level</param>
    /// <param name="weatherCondition">Filter by weather condition</param>
    /// <param name="search">Search term</param>
    /// <returns>Paginated list of weather risk analyses</returns>
    [HttpGet]
    public async Task<IActionResult> GetWeatherRiskAnalyses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? riskLevel = null,
        [FromQuery] string? weatherCondition = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _weatherRiskAnalysisService.GetAnalysesAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                riskLevel, 
                weatherCondition, 
                search);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving weather risk analyses: {ex.Message}");
        }
    }

    /// <summary>
    /// Get a specific weather risk analysis by ID
    /// </summary>
    /// <param name="id">Analysis ID</param>
    /// <returns>Weather risk analysis details</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetWeatherRiskAnalysis(Guid id)
    {
        try
        {
            var result = await _weatherRiskAnalysisService.GetAnalysisByIdAsync(id);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving weather risk analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new weather risk analysis
    /// </summary>
    /// <param name="createDto">Analysis creation data</param>
    /// <returns>Created weather risk analysis</returns>
    [HttpPost]
    public async Task<IActionResult> CreateWeatherRiskAnalysis([FromBody] CreateWeatherRiskAnalysisDto createDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid weather risk analysis data");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _weatherRiskAnalysisService.CreateAsync(createDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetWeatherRiskAnalysis), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error creating weather risk analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Update an existing weather risk analysis
    /// </summary>
    /// <param name="id">Analysis ID</param>
    /// <param name="updateDto">Analysis update data</param>
    /// <returns>Updated weather risk analysis</returns>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateWeatherRiskAnalysis(Guid id, [FromBody] UpdateWeatherRiskAnalysisDto updateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid weather risk analysis data");

            // Ensure the ID in the URL matches the DTO
            updateDto.Id = id;

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _weatherRiskAnalysisService.UpdateAsync(updateDto, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error updating weather risk analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Delete a weather risk analysis
    /// </summary>
    /// <param name="id">Analysis ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteWeatherRiskAnalysis(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _weatherRiskAnalysisService.DeleteAsync(id, currentUserId.Value);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error deleting weather risk analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Get weather risk analyses by project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="riskLevel">Filter by risk level</param>
    /// <returns>Paginated list of analyses for the project</returns>
    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetAnalysesByProject(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? riskLevel = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            var result = await _weatherRiskAnalysisService.GetAnalysesAsync(
                validPageNumber, 
                validPageSize, 
                projectId, 
                riskLevel);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project weather risk analyses: {ex.Message}");
        }
    }

    /// <summary>
    /// Generate weather risk analysis for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="generateDto">Generation parameters</param>
    /// <returns>Generated analysis</returns>
    [HttpPost("project/{projectId:guid}/generate")]
    public async Task<IActionResult> GenerateAnalysis(Guid projectId, [FromBody] GenerateAnalysisDto generateDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return ValidationErrorResponse("Invalid generation parameters");

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
                return UnauthorizedResponse("User authentication required");

            var result = await _weatherRiskAnalysisService.GenerateAnalysisAsync(projectId, generateDto, currentUserId.Value);
            
            if (result.Success && result.Data != null)
            {
                return CreatedAtAction(
                    nameof(GetWeatherRiskAnalysis), 
                    new { id = result.Data.Id }, 
                    result);
            }

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error generating weather risk analysis: {ex.Message}");
        }
    }

    /// <summary>
    /// Get high-risk analyses across all projects
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="organizationId">Filter by organization</param>
    /// <returns>Paginated list of high-risk analyses</returns>
    [HttpGet("high-risk")]
    public async Task<IActionResult> GetHighRiskAnalyses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? organizationId = null)
    {
        try
        {
            var (validPageNumber, validPageSize) = ValidatePaginationParameters(pageNumber, pageSize);
            
            // Use organization from claims if not provided
            organizationId ??= GetCurrentOrganizationId();

            var result = await _weatherRiskAnalysisService.GetHighRiskAnalysesAsync(
                validPageNumber, 
                validPageSize, 
                organizationId);

            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving high-risk analyses: {ex.Message}");
        }
    }

    /// <summary>
    /// Get weather risk summary for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Risk summary statistics</returns>
    [HttpGet("project/{projectId:guid}/summary")]
    public async Task<IActionResult> GetProjectRiskSummary(Guid projectId)
    {
        try
        {
            var result = await _weatherRiskAnalysisService.GetProjectRiskSummaryAsync(projectId);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving project risk summary: {ex.Message}");
        }
    }

    /// <summary>
    /// Get weather forecast data for analysis
    /// </summary>
    /// <param name="latitude">Latitude</param>
    /// <param name="longitude">Longitude</param>
    /// <param name="days">Number of days to forecast</param>
    /// <returns>Weather forecast data</returns>
    [HttpGet("forecast")]
    public async Task<IActionResult> GetWeatherForecast(
        [FromQuery] decimal latitude,
        [FromQuery] decimal longitude,
        [FromQuery] int days = 7)
    {
        try
        {
            if (latitude < -90 || latitude > 90)
                return ValidationErrorResponse("Latitude must be between -90 and 90");

            if (longitude < -180 || longitude > 180)
                return ValidationErrorResponse("Longitude must be between -180 and 180");

            if (days < 1 || days > 14)
                return ValidationErrorResponse("Days must be between 1 and 14");

            var result = await _weatherRiskAnalysisService.GetWeatherForecastAsync(latitude, longitude, days);
            return HandleServiceResponse(result);
        }
        catch (Exception ex)
        {
            return ServerErrorResponse($"Error retrieving weather forecast: {ex.Message}");
        }
    }
}

