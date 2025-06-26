using System.Text.Json;

namespace WeatherGuard.Core.Entities;

public class Task : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid ScheduleId { get; set; }
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? Duration { get; set; }
    public string? PredecessorIds { get; set; }
    public bool WeatherSensitive { get; set; } = false;
    public string? WeatherCategories { get; set; }

    // Hierarchy fields
    public Guid? ParentTaskId { get; set; }
    public string? WBSCode { get; set; }
    public int TaskLevel { get; set; } = 0;
    public int? SortOrder { get; set; }
    public string TaskType { get; set; } = "Task";
    public string? OutlineNumber { get; set; }
    public bool IsSummaryTask { get; set; } = false;
    public bool IsMilestone { get; set; } = false;

    // Project management fields
    public decimal PercentComplete { get; set; } = 0;
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public DateTime? BaselineStartDate { get; set; }
    public DateTime? BaselineEndDate { get; set; }
    public int? BaselineDuration { get; set; }
    public bool CriticalPath { get; set; } = false;
    public int TotalFloat { get; set; } = 0;
    public int FreeFloat { get; set; } = 0;

    // Cost tracking
    public decimal? PlannedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public decimal? RemainingCost { get; set; }

    // BIM integration
    public string? BIMElementIds { get; set; }
    public Guid? LocationElementId { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ProjectSchedule Schedule { get; set; } = null!;
    public virtual Task? ParentTask { get; set; }
    public virtual ICollection<Task> ChildTasks { get; set; } = new List<Task>();
    public virtual ICollection<TaskDependency> PredecessorDependencies { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TaskDependency> SuccessorDependencies { get; set; } = new List<TaskDependency>();
    public virtual ICollection<TaskRiskDetail> RiskDetails { get; set; } = new List<TaskRiskDetail>();
    public virtual BIMElement? LocationElement { get; set; }

    // Computed properties
    public int? DurationInDays => StartDate.HasValue && EndDate.HasValue 
        ? (int)(EndDate.Value - StartDate.Value).TotalDays 
        : Duration;

    public bool IsOverdue => EndDate.HasValue && DateTime.UtcNow > EndDate.Value && PercentComplete < 100;

    public bool IsInProgress => ActualStartDate.HasValue && !ActualEndDate.HasValue;

    public List<Guid> GetBIMElementIds()
    {
        if (string.IsNullOrEmpty(BIMElementIds))
            return new List<Guid>();

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(BIMElementIds) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    public void SetBIMElementIds(List<Guid> elementIds)
    {
        BIMElementIds = JsonSerializer.Serialize(elementIds);
    }

    public List<string> GetWeatherCategoriesList()
    {
        if (string.IsNullOrEmpty(WeatherCategories))
            return new List<string>();

        try
        {
            return JsonSerializer.Deserialize<List<string>>(WeatherCategories) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public void SetWeatherCategories(List<string> categories)
    {
        WeatherCategories = JsonSerializer.Serialize(categories);
    }
}