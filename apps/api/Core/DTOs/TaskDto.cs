using System.ComponentModel.DataAnnotations;

namespace WeatherGuard.Core.DTOs;

public class TaskDto : BaseDto
{
    public Guid ProjectId { get; set; }
    public Guid ScheduleId { get; set; }
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Duration { get; set; }
    public bool WeatherSensitive { get; set; }
    public List<string> WeatherCategories { get; set; } = new();
    
    // Hierarchy fields
    public Guid? ParentTaskId { get; set; }
    public string? WBSCode { get; set; }
    public int TaskLevel { get; set; }
    public int? SortOrder { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string? OutlineNumber { get; set; }
    public bool IsSummaryTask { get; set; }
    public bool IsMilestone { get; set; }
    
    // Project management fields
    public decimal PercentComplete { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? BaselineStartDate { get; set; }
    public DateTime? BaselineEndDate { get; set; }
    public int? BaselineDuration { get; set; }
    public bool CriticalPath { get; set; }
    public int TotalFloat { get; set; }
    public int FreeFloat { get; set; }
    
    // Cost tracking
    public decimal? PlannedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public decimal? RemainingCost { get; set; }
    
    // BIM integration
    public List<Guid> BIMElementIds { get; set; } = new();
    public Guid? LocationElementId { get; set; }
    
    // Computed properties
    public int? DurationInDays { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsInProgress { get; set; }
    
    // Navigation properties
    public string? ProjectName { get; set; }
    public string? ParentTaskName { get; set; }
    public int ChildTaskCount { get; set; }
    public int DependencyCount { get; set; }
}

public class CreateTaskDto : BaseCreateDto
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public Guid ScheduleId { get; set; }

    [StringLength(255)]
    public string? ExternalId { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? Duration { get; set; }

    public bool WeatherSensitive { get; set; } = false;
    public List<string> WeatherCategories { get; set; } = new();

    public Guid? ParentTaskId { get; set; }
    
    [StringLength(50)]
    public string TaskType { get; set; } = "Task";

    [Range(0, 100)]
    public decimal PercentComplete { get; set; } = 0;

    public DateTime? BaselineStartDate { get; set; }
    public DateTime? BaselineEndDate { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? BaselineDuration { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? PlannedCost { get; set; }

    public List<Guid> BIMElementIds { get; set; } = new();
    public Guid? LocationElementId { get; set; }
}

public class UpdateTaskDto : BaseUpdateDto
{
    [StringLength(500)]
    public string? Name { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    [Range(0, int.MaxValue)]
    public int? Duration { get; set; }

    public bool? WeatherSensitive { get; set; }
    public List<string>? WeatherCategories { get; set; }

    [Range(0, 100)]
    public decimal? PercentComplete { get; set; }

    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? ActualCost { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal? RemainingCost { get; set; }

    public List<Guid>? BIMElementIds { get; set; }
    public Guid? LocationElementId { get; set; }
}

public class TaskHierarchyDto : TaskDto
{
    public string IndentedName { get; set; } = string.Empty;
    public string HierarchyPath { get; set; } = string.Empty;
    public List<TaskHierarchyDto> Children { get; set; } = new();
}