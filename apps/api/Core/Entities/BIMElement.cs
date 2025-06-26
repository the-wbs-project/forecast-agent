using System.Text.Json;

namespace WeatherGuard.Core.Entities;

public class BIMElement : BaseEntity
{
    public Guid ModelId { get; set; }
    public string GlobalId { get; set; } = string.Empty;
    public int EntityTypeId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Tag { get; set; }
    public string? ElementType { get; set; }
    public string? MaterialIds { get; set; }
    public string? PropertySets { get; set; }
    public string? ClassificationCode { get; set; }
    public string? LevelOfDevelopment { get; set; }
    public Guid? ParentElementId { get; set; }
    public Guid? SpatialContainerId { get; set; }

    // Navigation properties
    public virtual BIMModel Model { get; set; } = null!;
    public virtual IFCEntityType EntityType { get; set; } = null!;
    public virtual BIMElement? ParentElement { get; set; }
    public virtual BIMElement? SpatialContainer { get; set; }
    public virtual ICollection<BIMElement> ChildElements { get; set; } = new List<BIMElement>();
    public virtual ICollection<BIMElement> ContainedElements { get; set; } = new List<BIMElement>();
    public virtual ICollection<BIMGeometry> Geometries { get; set; } = new List<BIMGeometry>();
    public virtual ICollection<BIMRelation> RelatingRelations { get; set; } = new List<BIMRelation>();
    public virtual ICollection<BIMRelation> RelatedRelations { get; set; } = new List<BIMRelation>();
    public virtual ICollection<Task> TasksAtLocation { get; set; } = new List<Task>();

    // Computed properties
    public List<Guid> GetMaterialIds()
    {
        if (string.IsNullOrEmpty(MaterialIds))
            return new List<Guid>();

        try
        {
            return JsonSerializer.Deserialize<List<Guid>>(MaterialIds) ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }

    public void SetMaterialIds(List<Guid> materialIds)
    {
        MaterialIds = JsonSerializer.Serialize(materialIds);
    }

    public Dictionary<string, object> GetPropertySets()
    {
        if (string.IsNullOrEmpty(PropertySets))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(PropertySets) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetPropertySets(Dictionary<string, object> propertySets)
    {
        PropertySets = JsonSerializer.Serialize(propertySets);
    }
}