using Microsoft.EntityFrameworkCore;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Api.Tests.Infrastructure.Data;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    // Only include entities needed for testing
    public DbSet<Firm> Firms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectSchedule> ProjectSchedules { get; set; }
    public DbSet<WeatherGuard.Core.Entities.Task> Tasks { get; set; }
    public DbSet<WeatherRiskAnalysis> WeatherRiskAnalyses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Clear all registered entity types and only add the ones we want
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().ToList())
        {
            //modelBuilder.Model.RemoveEntityType(entityType);
        }
        
        // Only add specific entities to prevent auto-discovery of BIM entities
        modelBuilder.Entity<Firm>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasOne<Firm>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne<Firm>()
                .WithMany()
                .HasForeignKey(e => e.OrganizationId);
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy);
            
            // Ignore all navigation properties that might reference BIM entities
            entity.Ignore(e => e.Organization);
            entity.Ignore(e => e.Creator);
            entity.Ignore(e => e.Schedules);
            entity.Ignore(e => e.Tasks);
            entity.Ignore(e => e.WeatherRiskAnalyses);
            entity.Ignore(e => e.BIMModels);
            entity.Ignore(e => e.CoordinationIssues);
        });

        modelBuilder.Entity<ProjectSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId);
        });

        modelBuilder.Entity<WeatherGuard.Core.Entities.Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId);
        });

        modelBuilder.Entity<WeatherRiskAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(e => e.ProjectId);
        });
    }
}