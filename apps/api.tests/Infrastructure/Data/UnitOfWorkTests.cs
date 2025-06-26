using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using WeatherGuard.Core.Entities;
using WeatherGuard.Core.Interfaces;

namespace WeatherGuard.Api.Tests.Infrastructure.Data;

public class UnitOfWorkTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly TestUnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        _context = new TestDbContext(options);
        _context.Database.EnsureCreated();
        _unitOfWork = new TestUnitOfWork(_context);
    }

    [Fact]
    public void UnitOfWork_ShouldInitializeMainRepositories()
    {
        // Assert - only test the repositories that are implemented in our test unit of work
        _unitOfWork.Firms.Should().NotBeNull();
        _unitOfWork.Users.Should().NotBeNull();
        _unitOfWork.Projects.Should().NotBeNull();
        _unitOfWork.ProjectSchedules.Should().NotBeNull();
        _unitOfWork.Tasks.Should().NotBeNull();
        _unitOfWork.WeatherRiskAnalyses.Should().NotBeNull();
    }

    [Fact]
    public void Repositories_ShouldReturnSameInstanceOnMultipleAccess()
    {
        // Act
        var projects1 = _unitOfWork.Projects;
        var projects2 = _unitOfWork.Projects;

        // Assert
        projects1.Should().BeSameAs(projects2);
    }

    [Fact]
    public async System.Threading.Tasks.Task SaveChangesAsync_WithAddedEntities_ShouldReturnCorrectCount()
    {
        // Arrange
        var firm = new Firm
        {
            Id = Guid.NewGuid(),
            Name = "Test Firm",
            Subdomain = "test-firm",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = firm.Id,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.Firms.AddAsync(firm);
        await _unitOfWork.Users.AddAsync(user);
        var result = await _unitOfWork.SaveChangesAsync();

        // Assert
        result.Should().Be(2);

        var savedFirm = await _unitOfWork.Firms.GetByIdAsync(firm.Id);
        var savedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);

        savedFirm.Should().NotBeNull();
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task Transaction_WithCommit_ShouldPersistChanges()
    {
        // Arrange
        var firm = new Firm
        {
            Id = Guid.NewGuid(),
            Name = "Test Firm",
            Subdomain = "test-firm",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Firms.AddAsync(firm);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        var savedFirm = await _unitOfWork.Firms.GetByIdAsync(firm.Id);
        savedFirm.Should().NotBeNull();
        savedFirm!.Name.Should().Be("Test Firm");
    }

    [Fact]
    public async System.Threading.Tasks.Task Transaction_WithRollback_ShouldNotPersistChanges()
    {
        // Arrange
        var firm = new Firm
        {
            Id = Guid.NewGuid(),
            Name = "Test Firm",
            Subdomain = "test-firm",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.Firms.AddAsync(firm);
        await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.RollbackTransactionAsync();

        // Assert
        var savedFirm = await _unitOfWork.Firms.GetByIdAsync(firm.Id);
        savedFirm.Should().BeNull();
    }

    [Fact]
    public async System.Threading.Tasks.Task MultipleOperations_WithinTransaction_ShouldWorkCorrectly()
    {
        // Arrange
        var firm = new Firm
        {
            Id = Guid.NewGuid(),
            Name = "Test Firm",
            Subdomain = "test-firm",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            OrganizationId = firm.Id,
            Email = "test@example.com",
            FirstName = "John",
            LastName = "Doe",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var project = new Project
        {
            Id = Guid.NewGuid(),
            OrganizationId = firm.Id,
            Name = "Test Project",
            CreatedBy = user.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        await _unitOfWork.BeginTransactionAsync();
        
        await _unitOfWork.Firms.AddAsync(firm);
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.Projects.AddAsync(project);
        
        var saveResult = await _unitOfWork.SaveChangesAsync();
        await _unitOfWork.CommitTransactionAsync();

        // Assert
        saveResult.Should().Be(3);

        var savedFirm = await _unitOfWork.Firms.GetByIdAsync(firm.Id);
        var savedUser = await _unitOfWork.Users.GetByIdAsync(user.Id);
        var savedProject = await _unitOfWork.Projects.GetByIdAsync(project.Id);

        savedFirm.Should().NotBeNull();
        savedUser.Should().NotBeNull();
        savedProject.Should().NotBeNull();
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
        _context.Dispose();
    }
}