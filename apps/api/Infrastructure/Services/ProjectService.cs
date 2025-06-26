using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.DTOs.Common;
using WeatherGuard.Core.Entities;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Infrastructure.Services;

public class ProjectService : IProjectService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProjectService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ApiResponseDto<ProjectDto>> GetProjectByIdAsync(Guid id)
    {
        try
        {
            var project = await _unitOfWork.Projects
                .GetQueryable(p => p.Organization, p => p.Creator)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return ApiResponseDto<ProjectDto>.ErrorResult("Project not found");
            }

            var projectDto = _mapper.Map<ProjectDto>(project);
            projectDto.OrganizationName = project.Organization.Name;
            projectDto.CreatorName = project.Creator.FullName;

            // Get counts
            projectDto.ScheduleCount = await _unitOfWork.ProjectSchedules.CountAsync(s => s.ProjectId == id);
            projectDto.TaskCount = await _unitOfWork.Tasks.CountAsync(t => t.ProjectId == id);
            projectDto.AnalysisCount = await _unitOfWork.WeatherRiskAnalyses.CountAsync(a => a.ProjectId == id);

            return ApiResponseDto<ProjectDto>.SuccessResult(projectDto);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProjectDto>.ErrorResult($"Error retrieving project: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<PagedResultDto<ProjectDto>>> GetProjectsAsync(
        int pageNumber, 
        int pageSize, 
        string? status = null, 
        Guid? organizationId = null, 
        string? search = null)
    {
        try
        {
            var query = _unitOfWork.Projects
                .GetQueryable(p => p.Organization, p => p.Creator);

            // Apply organization filter if provided
            if (organizationId.HasValue)
            {
                query = query.Where(p => p.OrganizationId == organizationId.Value);
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.Status == status);
            }

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(searchLower) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                    (p.Location != null && p.Location.ToLower().Contains(searchLower)));
            }

            // Apply default sorting
            query = query.OrderByDescending(p => p.CreatedAt);

            var totalCount = await query.CountAsync();
            var projects = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var projectDtos = new List<ProjectDto>();
            foreach (var project in projects)
            {
                var dto = _mapper.Map<ProjectDto>(project);
                dto.OrganizationName = project.Organization.Name;
                dto.CreatorName = project.Creator.FullName;
                
                // Get counts (could be optimized with a single query)
                dto.ScheduleCount = await _unitOfWork.ProjectSchedules.CountAsync(s => s.ProjectId == project.Id);
                dto.TaskCount = await _unitOfWork.Tasks.CountAsync(t => t.ProjectId == project.Id);
                dto.AnalysisCount = await _unitOfWork.WeatherRiskAnalyses.CountAsync(a => a.ProjectId == project.Id);
                
                projectDtos.Add(dto);
            }

            var pagedResult = new PagedResultDto<ProjectDto>
            {
                Items = projectDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };
            return ApiResponseDto<PagedResultDto<ProjectDto>>.SuccessResult(pagedResult);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<PagedResultDto<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProjectDto>> CreateAsync(CreateProjectDto dto, Guid createdBy)
    {
        try
        {
            // Validate organization exists
            var organizationExists = await _unitOfWork.Firms.ExistsAsync(f => f.Id == dto.OrganizationId);
            if (!organizationExists)
            {
                return ApiResponseDto<ProjectDto>.ErrorResult("Organization not found");
            }

            var project = _mapper.Map<Project>(dto);
            project.Id = Guid.NewGuid();
            project.CreatedBy = createdBy;
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Projects.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return await GetProjectByIdAsync(project.Id);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProjectDto>.ErrorResult($"Error creating project: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProjectDto>> UpdateAsync(UpdateProjectDto dto, Guid updatedBy)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(dto.Id);
            if (project == null)
            {
                return ApiResponseDto<ProjectDto>.ErrorResult("Project not found");
            }

            // Map only non-null properties
            if (!string.IsNullOrEmpty(dto.Name))
                project.Name = dto.Name;
            
            if (dto.Description != null)
                project.Description = dto.Description;
            
            if (dto.Location != null)
                project.Location = dto.Location;
            
            if (dto.Latitude.HasValue)
                project.Latitude = dto.Latitude;
            
            if (dto.Longitude.HasValue)
                project.Longitude = dto.Longitude;
            
            if (dto.StartDate.HasValue)
                project.StartDate = dto.StartDate;
            
            if (dto.EndDate.HasValue)
                project.EndDate = dto.EndDate;
            
            if (!string.IsNullOrEmpty(dto.Status))
                project.Status = dto.Status;
            
            if (dto.BIMEnabled.HasValue)
                project.BIMEnabled = dto.BIMEnabled.Value;
            
            if (dto.CoordinateSystemId != null)
                project.CoordinateSystemId = dto.CoordinateSystemId;
            
            if (dto.NorthDirection.HasValue)
                project.NorthDirection = dto.NorthDirection;
            
            if (dto.BuildingHeight.HasValue)
                project.BuildingHeight = dto.BuildingHeight;
            
            if (dto.GrossFloorArea.HasValue)
                project.GrossFloorArea = dto.GrossFloorArea;
            
            if (dto.NumberOfStoreys.HasValue)
                project.NumberOfStoreys = dto.NumberOfStoreys;

            project.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Projects.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return await GetProjectByIdAsync(dto.Id);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProjectDto>.ErrorResult($"Error updating project: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteAsync(Guid id, Guid deletedBy)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Project not found");
            }

            // Check if project has dependencies
            var hasSchedules = await _unitOfWork.ProjectSchedules.ExistsAsync(s => s.ProjectId == id);
            var hasTasks = await _unitOfWork.Tasks.ExistsAsync(t => t.ProjectId == id);
            
            if (hasSchedules || hasTasks)
            {
                return ApiResponseDto<bool>.ErrorResult("Cannot delete project with existing schedules or tasks");
            }

            await _unitOfWork.Projects.DeleteAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponseDto<bool>.SuccessResult(true);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResult($"Error deleting project: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<IEnumerable<ProjectDto>>> GetByOrganizationAsync(Guid organizationId)
    {
        try
        {
            var projects = await _unitOfWork.Projects
                .GetQueryable(p => p.Organization, p => p.Creator)
                .Where(p => p.OrganizationId == organizationId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var projectDtos = _mapper.Map<List<ProjectDto>>(projects);
            
            // Set navigation properties
            for (int i = 0; i < projects.Count; i++)
            {
                projectDtos[i].OrganizationName = projects[i].Organization.Name;
                projectDtos[i].CreatorName = projects[i].Creator.FullName;
            }

            return ApiResponseDto<IEnumerable<ProjectDto>>.SuccessResult(projectDtos);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<IEnumerable<ProjectDto>>.ErrorResult($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<bool>> ExistsAsync(Guid id)
    {
        try
        {
            var exists = await _unitOfWork.Projects.ExistsAsync(p => p.Id == id);
            return ApiResponseDto<bool>.SuccessResult(exists);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<bool>.ErrorResult($"Error checking project existence: {ex.Message}");
        }
    }

    public async Task<ApiResponseDto<ProjectStatsDto>> GetProjectStatsAsync(Guid id)
    {
        try
        {
            var project = await _unitOfWork.Projects.GetByIdAsync(id);
            if (project == null)
            {
                return ApiResponseDto<ProjectStatsDto>.ErrorResult("Project not found");
            }

            var totalTasks = await _unitOfWork.Tasks.CountAsync(t => t.ProjectId == id);
            var completedTasks = await _unitOfWork.Tasks.CountAsync(t => t.ProjectId == id && t.ActualEndDate.HasValue);
            var pendingTasks = totalTasks - completedTasks;
            var overdueTasks = await _unitOfWork.Tasks.CountAsync(t => t.ProjectId == id && 
                t.EndDate.HasValue && t.EndDate.Value < DateTime.UtcNow && !t.ActualEndDate.HasValue);

            var totalSchedules = await _unitOfWork.ProjectSchedules.CountAsync(s => s.ProjectId == id);
            var totalAnalyses = await _unitOfWork.WeatherRiskAnalyses.CountAsync(a => a.ProjectId == id);

            var completionPercentage = totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;

            var stats = new ProjectStatsDto
            {
                TotalTasks = totalTasks,
                CompletedTasks = completedTasks,
                PendingTasks = pendingTasks,
                OverdueTasks = overdueTasks,
                TotalSchedules = totalSchedules,
                TotalAnalyses = totalAnalyses,
                CompletionPercentage = completionPercentage,
                EstimatedCompletionDate = project.EndDate?.ToDateTime(TimeOnly.MinValue)
            };

            return ApiResponseDto<ProjectStatsDto>.SuccessResult(stats);
        }
        catch (Exception ex)
        {
            return ApiResponseDto<ProjectStatsDto>.ErrorResult($"Error retrieving project statistics: {ex.Message}");
        }
    }
}