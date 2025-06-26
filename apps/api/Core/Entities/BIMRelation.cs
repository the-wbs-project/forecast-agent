using System.Text.Json;

namespace WeatherGuard.Core.Entities;

public class BIMRelation : BaseEntity
{
    public string RelationType { get; set; } = string.Empty;
    public string GlobalId { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Description { get; set; }
    public Guid RelatingElementId { get; set; }
    public Guid RelatedElementId { get; set; }
    public string? Properties { get; set; }

    // Navigation properties
    public virtual BIMElement RelatingElement { get; set; } = null!;
    public virtual BIMElement RelatedElement { get; set; } = null!;

    // Computed properties
    public Dictionary<string, object> GetProperties()
    {
        if (string.IsNullOrEmpty(Properties))
            return new Dictionary<string, object>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(Properties) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    public void SetProperties(Dictionary<string, object> properties)
    {
        Properties = JsonSerializer.Serialize(properties);
    }
}