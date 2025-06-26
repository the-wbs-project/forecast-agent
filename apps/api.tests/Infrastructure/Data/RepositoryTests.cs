using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Api.Tests.Infrastructure.Data;

public class RepositoryTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly TestRepository<Project> _repository;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
        _repository = new TestRepository<Project>(_context);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_WithExistingId_ShouldReturnEntity()
    {
        // Arrange
        var project = CreateTestProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(project.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
        result.Name.Should().Be(project.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetAllAsync_WithMultipleEntities_ShouldReturnAllEntities()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        var project2 = CreateTestProject("Project 2");
        var project3 = CreateTestProject("Project 3");

        await _context.Projects.AddRangeAsync(new[] { project1, project2, project3 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Select(p => p.Name).Should().BeEquivalentTo(new[] { "Project 1", "Project 2", "Project 3" });
    }

    [Fact]
    public async System.Threading.Tasks.Task FindAsync_WithValidPredicate_ShouldReturnMatchingEntities()
    {
        // Arrange
        var project1 = CreateTestProject("Active Project 1");
        project1.Status = "Active";
        var project2 = CreateTestProject("Active Project 2");
        project2.Status = "Active";
        var project3 = CreateTestProject("Completed Project");
        project3.Status = "Completed";

        await _context.Projects.AddRangeAsync(new[] { project1, project2, project3 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindAsync(p => p.Status == "Active");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Status == "Active");
    }

    [Fact]
    public async System.Threading.Tasks.Task FirstOrDefaultAsync_WithMatchingPredicate_ShouldReturnFirstMatch()
    {
        // Arrange
        var project1 = CreateTestProject("Project A");
        project1.Status = "Active";
        var project2 = CreateTestProject("Project B");
        project2.Status = "Active";

        await _context.Projects.AddRangeAsync(new[] { project1, project2 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(p => p.Status == "Active");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
    }

    [Fact]
    public async System.Threading.Tasks.Task FirstOrDefaultAsync_WithNonMatchingPredicate_ShouldReturnNull()
    {
        // Arrange
        var project = CreateTestProject();
        project.Status = "Active";
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FirstOrDefaultAsync(p => p.Status == "NonExistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExistsAsync_WithMatchingPredicate_ShouldReturnTrue()
    {
        // Arrange
        var project = CreateTestProject();
        project.Status = "Active";
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(p => p.Status == "Active");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExistsAsync_WithNonMatchingPredicate_ShouldReturnFalse()
    {
        // Arrange
        var project = CreateTestProject();
        project.Status = "Active";
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ExistsAsync(p => p.Status == "NonExistent");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async System.Threading.Tasks.Task CountAsync_WithoutPredicate_ShouldReturnTotalCount()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        var project2 = CreateTestProject("Project 2");
        var project3 = CreateTestProject("Project 3");

        await _context.Projects.AddRangeAsync(new[] { project1, project2, project3 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync();

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async System.Threading.Tasks.Task CountAsync_WithPredicate_ShouldReturnMatchingCount()
    {
        // Arrange
        var project1 = CreateTestProject("Project 1");
        project1.Status = "Active";
        var project2 = CreateTestProject("Project 2");
        project2.Status = "Active";
        var project3 = CreateTestProject("Project 3");
        project3.Status = "Completed";

        await _context.Projects.AddRangeAsync(new[] { project1, project2, project3 });
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.CountAsync(p => p.Status == "Active");

        // Assert
        result.Should().Be(2);
    }

    [Fact]
    public async System.Threading.Tasks.Task AddAsync_WithValidEntity_ShouldAddEntity()
    {
        // Arrange
        var project = CreateTestProject();

        // Act
        var result = await _repository.AddAsync(project);
        await _context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(project.Id);

        var savedProject = await _context.Projects.FindAsync(project.Id);
        savedProject.Should().NotBeNull();
        savedProject!.Name.Should().Be(project.Name);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateAsync_WithExistingEntity_ShouldUpdateEntity()
    {
        // Arrange
        var project = CreateTestProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Modify the project
        project.Name = "Updated Project Name";
        project.Status = "Completed";

        // Act
        await _repository.UpdateAsync(project);
        await _context.SaveChangesAsync();

        // Assert
        var updatedProject = await _context.Projects.FindAsync(project.Id);
        updatedProject.Should().NotBeNull();
        updatedProject!.Name.Should().Be("Updated Project Name");
        updatedProject.Status.Should().Be("Completed");
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteAsync_WithExistingEntity_ShouldRemoveEntity()
    {
        // Arrange
        var project = CreateTestProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(project);
        await _context.SaveChangesAsync();

        // Assert
        var deletedProject = await _context.Projects.FindAsync(project.Id);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteByIdAsync_WithExistingId_ShouldRemoveEntity()
    {
        // Arrange
        var project = CreateTestProject();
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteByIdAsync(project.Id);
        await _context.SaveChangesAsync();

        // Assert
        var deletedProject = await _context.Projects.FindAsync(project.Id);
        deletedProject.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task GetPagedAsync_WithValidParameters_ShouldReturnPagedResults()
    {
        // Arrange
        var projects = Enumerable.Range(1, 15)
            .Select(i => CreateTestProject($"Project {i:D2}"))
            .ToArray();

        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: 2,
            pageSize: 5,
            orderBy: query => query.OrderBy(p => p.Name));

        // Assert
        totalCount.Should().Be(15);
        items.Should().HaveCount(5);
        items.First().Name.Should().Be("Project 06");
        items.Last().Name.Should().Be("Project 10");
    }

    [Fact]
    public async System.Threading.Tasks.Task GetPagedAsync_WithPredicate_ShouldReturnFilteredResults()
    {
        // Arrange
        var project1 = CreateTestProject("Active Project 1");
        project1.Status = "Active";
        var project2 = CreateTestProject("Active Project 2");
        project2.Status = "Active";
        var project3 = CreateTestProject("Completed Project 1");
        project3.Status = "Completed";
        var project4 = CreateTestProject("Completed Project 2");
        project4.Status = "Completed";
        var project5 = CreateTestProject("Cancelled Project");
        project5.Status = "Cancelled";
        
        var projects = new[] { project1, project2, project3, project4, project5 };

        await _context.Projects.AddRangeAsync(projects);
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(
            pageNumber: 1,
            pageSize: 10,
            predicate: p => p.Status == "Active",
            orderBy: query => query.OrderBy(p => p.Name));

        // Assert
        totalCount.Should().Be(2);
        items.Should().HaveCount(2);
        items.Should().OnlyContain(p => p.Status == "Active");
    }

    private Project CreateTestProject(string name = "Test Project")
    {
        return new Project
        {
            Id = Guid.NewGuid(),
            Name = name,
            OrganizationId = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}