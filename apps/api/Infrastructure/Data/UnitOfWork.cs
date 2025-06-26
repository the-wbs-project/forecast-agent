using Microsoft.EntityFrameworkCore.Storage;
using WeatherGuard.Core.Entities;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly WeatherGuardDbContext _context;
    private IDbContextTransaction? _transaction;

    // Core repositories
    private IRepository<Firm>? _firms;
    private IRepository<User>? _users;
    private IRepository<Project>? _projects;
    private IRepository<ProjectSchedule>? _projectSchedules;
    private IRepository<Core.Entities.Task>? _tasks;
    private IRepository<TaskDependency>? _taskDependencies;
    private IRepository<WeatherRiskAnalysis>? _weatherRiskAnalyses;
    private IRepository<TaskRiskDetail>? _taskRiskDetails;
    private IRepository<AuditLog>? _auditLogs;

    // BIM repositories
    private IRepository<IFCEntityType>? _ifcEntityTypes;
    private IRepository<BIMModel>? _bimModels;
    private IRepository<BIMElement>? _bimElements;
    private IRepository<BIMGeometry>? _bimGeometries;
    private IRepository<BIMRelation>? _bimRelations;
    private IRepository<BIMClashDetection>? _bimClashDetections;
    private IRepository<BIMModelVersion>? _bimModelVersions;
    private IRepository<BIMCoordinationIssue>? _bimCoordinationIssues;

    public UnitOfWork(WeatherGuardDbContext context)
    {
        _context = context;
    }

    // Core repositories
    public IRepository<Firm> Firms => _firms ??= new Repository<Firm>(_context);
    public IRepository<User> Users => _users ??= new Repository<User>(_context);
    public IRepository<Project> Projects => _projects ??= new Repository<Project>(_context);
    public IRepository<ProjectSchedule> ProjectSchedules => _projectSchedules ??= new Repository<ProjectSchedule>(_context);
    public IRepository<Core.Entities.Task> Tasks => _tasks ??= new Repository<Core.Entities.Task>(_context);
    public IRepository<TaskDependency> TaskDependencies => _taskDependencies ??= new Repository<TaskDependency>(_context);
    public IRepository<WeatherRiskAnalysis> WeatherRiskAnalyses => _weatherRiskAnalyses ??= new Repository<WeatherRiskAnalysis>(_context);
    public IRepository<TaskRiskDetail> TaskRiskDetails => _taskRiskDetails ??= new Repository<TaskRiskDetail>(_context);
    public IRepository<AuditLog> AuditLogs => _auditLogs ??= new Repository<AuditLog>(_context);

    // BIM repositories
    public IRepository<IFCEntityType> IFCEntityTypes => _ifcEntityTypes ??= new Repository<IFCEntityType>(_context);
    public IRepository<BIMModel> BIMModels => _bimModels ??= new Repository<BIMModel>(_context);
    public IRepository<BIMElement> BIMElements => _bimElements ??= new Repository<BIMElement>(_context);
    public IRepository<BIMGeometry> BIMGeometries => _bimGeometries ??= new Repository<BIMGeometry>(_context);
    public IRepository<BIMRelation> BIMRelations => _bimRelations ??= new Repository<BIMRelation>(_context);
    public IRepository<BIMClashDetection> BIMClashDetections => _bimClashDetections ??= new Repository<BIMClashDetection>(_context);
    public IRepository<BIMModelVersion> BIMModelVersions => _bimModelVersions ??= new Repository<BIMModelVersion>(_context);
    public IRepository<BIMCoordinationIssue> BIMCoordinationIssues => _bimCoordinationIssues ??= new Repository<BIMCoordinationIssue>(_context);

    public async System.Threading.Tasks.Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async System.Threading.Tasks.Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async System.Threading.Tasks.Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}