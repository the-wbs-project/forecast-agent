namespace WeatherGuard.Core.Entities;

public class TaskDependency : BaseEntity
{
    public Guid PredecessorTaskId { get; set; }
    public Guid SuccessorTaskId { get; set; }
    public string DependencyType { get; set; } = "FS"; // FS, FF, SF, SS
    public int LagDays { get; set; } = 0;

    // Navigation properties
    public virtual Task PredecessorTask { get; set; } = null!;
    public virtual Task SuccessorTask { get; set; } = null!;

    // Computed properties
    public string DependencyTypeDescription => DependencyType switch
    {
        "FS" => "Finish-to-Start",
        "FF" => "Finish-to-Finish",
        "SF" => "Start-to-Finish",
        "SS" => "Start-to-Start",
        _ => "Unknown"
    };
}