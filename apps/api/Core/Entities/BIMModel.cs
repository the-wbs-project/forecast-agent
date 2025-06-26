namespace WeatherGuard.Core.Entities;

public class BIMModel : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string GlobalId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AuthoringToolId { get; set; }
    public string? AuthoringToolVersion { get; set; }
    public string IFCVersion { get; set; } = "IFC4";
    public int ModelVersion { get; set; } = 1;
    public string? ModelPurpose { get; set; }
    public string? LevelOfDevelopment { get; set; }
    public Guid CreatedBy { get; set; }
    public string? FileReference { get; set; }

    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User Creator { get; set; } = null!;
    public virtual ICollection<BIMElement> Elements { get; set; } = new List<BIMElement>();
    public virtual ICollection<BIMModelVersion> Versions { get; set; } = new List<BIMModelVersion>();
    public virtual ICollection<BIMClashDetection> ClashDetections { get; set; } = new List<BIMClashDetection>();
}