using System.Text.Json;

namespace WeatherGuard.Core.Entities;

public class BIMCoordinationIssue : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string? IssueType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? ElementIds { get; set; }
    public string? ViewpointData { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User? AssignedUser { get; set; }
    public virtual User Creator { get; set; } = null!;

    // Computed properties
    public bool IsResolved => ResolvedAt.HasValue;
    public bool IsOverdue => DueDate.HasValue && DateTime.UtcNow > DueDate.Value && !IsResolved;

    public List<Guid> GetElementIds()
    {
        if (string.IsNullOrEmpty(ElementIds))
            return new List<Guid>();

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(ElementIds) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    public void SetElementIds(List<Guid> elementIds)
    {
        ElementIds = JsonSerializer.Serialize(elementIds);
    }

    public Dictionary<string, object> GetViewpointData()
    {
        if (string.IsNullOrEmpty(ViewpointData))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(ViewpointData) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetViewpointData(Dictionary<string, object> viewpointData)
    {
        ViewpointData = JsonSerializer.Serialize(viewpointData);
    }
}