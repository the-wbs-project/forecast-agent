namespace WeatherGuard.Core.Entities;

public class IFCEntityType
{
    public int Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? ParentType { get; set; }
    public string? Category { get; set; }
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<BIMElement> BIMElements { get; set; } = new List<BIMElement>();
}