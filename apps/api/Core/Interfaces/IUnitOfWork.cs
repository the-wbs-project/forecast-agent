using WeatherGuard.Core.Entities;

namespace WeatherGuard.Core.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Core repositories
    IRepository<Firm> Firms { get; }
    IRepository<User> Users { get; }
    IRepository<Project> Projects { get; }
    IRepository<ProjectSchedule> ProjectSchedules { get; }
    IRepository<Core.Entities.Task> Tasks { get; }
    IRepository<TaskDependency> TaskDependencies { get; }
    IRepository<WeatherRiskAnalysis> WeatherRiskAnalyses { get; }
    IRepository<TaskRiskDetail> TaskRiskDetails { get; }
    IRepository<AuditLog> AuditLogs { get; }

    // BIM repositories
    IRepository<IFCEntityType> IFCEntityTypes { get; }
    IRepository<BIMModel> BIMModels { get; }
    IRepository<BIMElement> BIMElements { get; }
    IRepository<BIMGeometry> BIMGeometries { get; }
    IRepository<BIMRelation> BIMRelations { get; }
    IRepository<BIMClashDetection> BIMClashDetections { get; }
    IRepository<BIMModelVersion> BIMModelVersions { get; }
    IRepository<BIMCoordinationIssue> BIMCoordinationIssues { get; }

    System.Threading.Tasks.Task<int> SaveChangesAsync();
    System.Threading.Tasks.Task BeginTransactionAsync();
    System.Threading.Tasks.Task CommitTransactionAsync();
    System.Threading.Tasks.Task RollbackTransactionAsync();
}