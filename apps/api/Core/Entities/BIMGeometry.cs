namespace WeatherGuard.Core.Entities;

public class BIMGeometry : BaseEntity
{
    public Guid ElementId { get; set; }
    public string? GeometryType { get; set; }
    public string? CoordinateSystem { get; set; }
    public string? BoundingBoxMin { get; set; }  // Store as WKT string for now
    public string? BoundingBoxMax { get; set; }  // Store as WKT string for now
    public string? Centroid { get; set; }        // Store as WKT string for now
    public decimal? Volume { get; set; }
    public decimal? Area { get; set; }
    public string? GeometryData { get; set; }

    // Navigation properties
    public virtual BIMElement Element { get; set; } = null!;
}