using Microsoft.EntityFrameworkCore;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Infrastructure.Data;

public class WeatherGuardDbContext : DbContext
{
    public WeatherGuardDbContext(DbContextOptions<WeatherGuardDbContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<Firm> Firms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectSchedule> ProjectSchedules { get; set; }
    public DbSet<Core.Entities.Task> Tasks { get; set; }
    public DbSet<TaskDependency> TaskDependencies { get; set; }
    public DbSet<WeatherRiskAnalysis> WeatherRiskAnalyses { get; set; }
    public DbSet<TaskRiskDetail> TaskRiskDetails { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    // BIM entities
    public DbSet<IFCEntityType> IFCEntityTypes { get; set; }
    public DbSet<BIMModel> BIMModels { get; set; }
    public DbSet<BIMElement> BIMElements { get; set; }
    public DbSet<BIMGeometry> BIMGeometries { get; set; }
    public DbSet<BIMRelation> BIMRelations { get; set; }
    public DbSet<BIMClashDetection> BIMClashDetections { get; set; }
    public DbSet<BIMModelVersion> BIMModelVersions { get; set; }
    public DbSet<BIMCoordinationIssue> BIMCoordinationIssues { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure entities
        ConfigureFirm(modelBuilder);
        ConfigureUser(modelBuilder);
        ConfigureProject(modelBuilder);
        ConfigureProjectSchedule(modelBuilder);
        ConfigureTask(modelBuilder);
        ConfigureTaskDependency(modelBuilder);
        ConfigureWeatherRiskAnalysis(modelBuilder);
        ConfigureTaskRiskDetail(modelBuilder);
        ConfigureAuditLog(modelBuilder);
        ConfigureBIMEntities(modelBuilder);
    }

    private static void ConfigureFirm(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });
    }

    private static void ConfigureUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("User");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Organization)
                .WithMany(f => f.Users)
                .HasForeignKey(e => e.OrganizationId);
        });
    }

    private static void ConfigureProject(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.Location).HasMaxLength(500);
            entity.Property(e => e.Latitude).HasPrecision(10, 8);
            entity.Property(e => e.Longitude).HasPrecision(11, 8);
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Active");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // BIM fields
            entity.Property(e => e.BIMEnabled).HasDefaultValue(false);
            entity.Property(e => e.CoordinateSystemId).HasMaxLength(50);
            entity.Property(e => e.NorthDirection).HasPrecision(5, 2);
            entity.Property(e => e.BuildingHeight).HasPrecision(10, 2);
            entity.Property(e => e.GrossFloorArea).HasPrecision(18, 2);

            entity.HasOne(e => e.Organization)
                .WithMany(f => f.Projects)
                .HasForeignKey(e => e.OrganizationId);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.CreatedProjects)
                .HasForeignKey(e => e.CreatedBy);
        });
    }

    private static void ConfigureProjectSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProjectSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.FileContent).HasColumnType("VARBINARY(MAX)");
            entity.Property(e => e.ParsedData).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
            entity.Property(e => e.ErrorMessage).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UploadedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Schedules)
                .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.UploadedByUser)
                .WithMany(u => u.UploadedSchedules)
                .HasForeignKey(e => e.UploadedBy);
        });
    }

    private static void ConfigureTask(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Core.Entities.Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExternalId).HasMaxLength(255);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.PredecessorIds).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.WeatherSensitive).HasDefaultValue(false);
            entity.Property(e => e.WeatherCategories).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Hierarchy fields
            entity.Property(e => e.WBSCode).HasMaxLength(50);
            entity.Property(e => e.TaskLevel).HasDefaultValue(0);
            entity.Property(e => e.TaskType).HasMaxLength(50).HasDefaultValue("Task");
            entity.Property(e => e.OutlineNumber).HasMaxLength(100);
            entity.Property(e => e.IsSummaryTask).HasDefaultValue(false);
            entity.Property(e => e.IsMilestone).HasDefaultValue(false);

            // Project management fields
            entity.Property(e => e.PercentComplete).HasPrecision(5, 2).HasDefaultValue(0);
            entity.Property(e => e.CriticalPath).HasDefaultValue(false);
            entity.Property(e => e.TotalFloat).HasDefaultValue(0);
            entity.Property(e => e.FreeFloat).HasDefaultValue(0);

            // Cost fields
            entity.Property(e => e.PlannedCost).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
            entity.Property(e => e.RemainingCost).HasPrecision(18, 2);

            // BIM fields
            entity.Property(e => e.BIMElementIds).HasColumnType("NVARCHAR(MAX)");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.Tasks)
                .HasForeignKey(e => e.ScheduleId);

            entity.HasOne(e => e.ParentTask)
                .WithMany(t => t.ChildTasks)
                .HasForeignKey(e => e.ParentTaskId);

            entity.HasOne(e => e.LocationElement)
                .WithMany(be => be.TasksAtLocation)
                .HasForeignKey(e => e.LocationElementId);
        });
    }

    private static void ConfigureTaskDependency(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskDependency>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DependencyType).HasMaxLength(10).HasDefaultValue("FS");
            entity.Property(e => e.LagDays).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => new { e.PredecessorTaskId, e.SuccessorTaskId }).IsUnique();

            entity.HasOne(e => e.PredecessorTask)
                .WithMany(t => t.SuccessorDependencies)
                .HasForeignKey(e => e.PredecessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.SuccessorTask)
                .WithMany(t => t.PredecessorDependencies)
                .HasForeignKey(e => e.SuccessorTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureWeatherRiskAnalysis(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeatherRiskAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.WeatherDataSource).HasMaxLength(100);
            entity.Property(e => e.RiskScore).HasPrecision(5, 2);
            entity.Property(e => e.TotalCostImpact).HasPrecision(18, 2);
            entity.Property(e => e.AnalysisResults).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.AnalysisDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.WeatherRiskAnalyses)
                .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.Schedule)
                .WithMany(s => s.WeatherRiskAnalyses)
                .HasForeignKey(e => e.ScheduleId);

            entity.HasOne(e => e.GeneratedByUser)
                .WithMany(u => u.GeneratedAnalyses)
                .HasForeignKey(e => e.GeneratedBy);
        });
    }

    private static void ConfigureTaskRiskDetail(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskRiskDetail>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RiskType).HasMaxLength(100);
            entity.Property(e => e.Probability).HasPrecision(5, 2);
            entity.Property(e => e.ImpactCost).HasPrecision(18, 2);
            entity.Property(e => e.MitigationSuggestions).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.WeatherForecast).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Analysis)
                .WithMany(a => a.TaskRiskDetails)
                .HasForeignKey(e => e.AnalysisId);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.RiskDetails)
                .HasForeignKey(e => e.TaskId);
        });
    }

    private static void ConfigureAuditLog(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(100);
            entity.Property(e => e.OldValues).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.NewValues).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Timestamp).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.User)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.UserId);
        });
    }

    private static void ConfigureBIMEntities(ModelBuilder modelBuilder)
    {
        // IFCEntityType
        modelBuilder.Entity<IFCEntityType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.EntityType).IsUnique();
            entity.Property(e => e.ParentType).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // BIMModel
        modelBuilder.Entity<BIMModel>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GlobalId).IsRequired().HasMaxLength(22);
            entity.HasIndex(e => e.GlobalId).IsUnique();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.AuthoringToolId).HasMaxLength(255);
            entity.Property(e => e.AuthoringToolVersion).HasMaxLength(50);
            entity.Property(e => e.IFCVersion).HasMaxLength(20).HasDefaultValue("IFC4");
            entity.Property(e => e.ModelVersion).HasDefaultValue(1);
            entity.Property(e => e.ModelPurpose).HasMaxLength(100);
            entity.Property(e => e.LevelOfDevelopment).HasMaxLength(10);
            entity.Property(e => e.FileReference).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Project)
                .WithMany(p => p.BIMModels)
                .HasForeignKey(e => e.ProjectId);

            entity.HasOne(e => e.Creator)
                .WithMany(u => u.CreatedBIMModels)
                .HasForeignKey(e => e.CreatedBy);
        });

        // BIMElement
        modelBuilder.Entity<BIMElement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GlobalId).IsRequired().HasMaxLength(22);
            entity.HasIndex(e => e.GlobalId).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(255);
            entity.Property(e => e.Description).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.Tag).HasMaxLength(100);
            entity.Property(e => e.ElementType).HasMaxLength(255);
            entity.Property(e => e.MaterialIds).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.PropertySets).HasColumnType("NVARCHAR(MAX)");
            entity.Property(e => e.ClassificationCode).HasMaxLength(100);
            entity.Property(e => e.LevelOfDevelopment).HasMaxLength(10);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(e => e.Model)
                .WithMany(m => m.Elements)
                .HasForeignKey(e => e.ModelId);

            entity.HasOne(e => e.EntityType)
                .WithMany(et => et.BIMElements)
                .HasForeignKey(e => e.EntityTypeId);

            entity.HasOne(e => e.ParentElement)
                .WithMany(be => be.ChildElements)
                .HasForeignKey(e => e.ParentElementId);

            entity.HasOne(e => e.SpatialContainer)
                .WithMany(be => be.ContainedElements)
                .HasForeignKey(e => e.SpatialContainerId);
        });
    }
}