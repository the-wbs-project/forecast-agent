using WeatherGuard.Core.Entities;
using WeatherGuard.Core.Interfaces;
using TaskEntity = WeatherGuard.Core.Entities.Task;

namespace WeatherGuard.Api.Tests.Infrastructure.Data;

public class TestUnitOfWork : IUnitOfWork
{
    private readonly TestDbContext _context;
    private bool _disposed = false;

    // Core repositories
    private TestRepository<Firm>? _firms;
    private TestRepository<User>? _users;
    private TestRepository<Project>? _projects;
    private TestRepository<ProjectSchedule>? _projectSchedules;
    private TestRepository<TaskEntity>? _tasks;
    private TestRepository<WeatherRiskAnalysis>? _weatherRiskAnalyses;

    public TestUnitOfWork(TestDbContext context)
    {
        _context = context;
    }

    public IRepository<Firm> Firms => _firms ??= new TestRepository<Firm>(_context);
    public IRepository<User> Users => _users ??= new TestRepository<User>(_context);
    public IRepository<Project> Projects => _projects ??= new TestRepository<Project>(_context);
    public IRepository<ProjectSchedule> ProjectSchedules => _projectSchedules ??= new TestRepository<ProjectSchedule>(_context);
    public IRepository<TaskEntity> Tasks => _tasks ??= new TestRepository<TaskEntity>(_context);
    public IRepository<WeatherRiskAnalysis> WeatherRiskAnalyses => _weatherRiskAnalyses ??= new TestRepository<WeatherRiskAnalysis>(_context);

    // Placeholder implementations for interfaces not needed in these tests
    public IRepository<TaskDependency> TaskDependencies => throw new NotImplementedException("Not needed for tests");
    public IRepository<TaskRiskDetail> TaskRiskDetails => throw new NotImplementedException("Not needed for tests");
    public IRepository<AuditLog> AuditLogs => throw new NotImplementedException("Not needed for tests");
    public IRepository<IFCEntityType> IFCEntityTypes => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMModel> BIMModels => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMElement> BIMElements => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMGeometry> BIMGeometries => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMRelation> BIMRelations => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMClashDetection> BIMClashDetections => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMModelVersion> BIMModelVersions => throw new NotImplementedException("Not needed for tests");
    public IRepository<BIMCoordinationIssue> BIMCoordinationIssues => throw new NotImplementedException("Not needed for tests");

    public async System.Threading.Tasks.Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async System.Threading.Tasks.Task CommitTransactionAsync()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CurrentTransaction.CommitAsync();
        }
    }

    public async System.Threading.Tasks.Task RollbackTransactionAsync()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            await _context.Database.CurrentTransaction.RollbackAsync();
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context.Dispose();
            _disposed = true;
        }
    }
}