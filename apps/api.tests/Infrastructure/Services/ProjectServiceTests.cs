using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using WeatherGuard.Core.DTOs;
using WeatherGuard.Core.Entities;
using WeatherGuard.Core.Interfaces;
using WeatherGuard.Infrastructure.Services;

namespace WeatherGuard.Api.Tests.Infrastructure.Services;

public class ProjectServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IRepository<Project>> _mockProjectRepository;
    private readonly Mock<IRepository<Firm>> _mockFirmRepository;
    private readonly Mock<IRepository<ProjectSchedule>> _mockScheduleRepository;
    private readonly Mock<IRepository<WeatherGuard.Core.Entities.Task>> _mockTaskRepository;
    private readonly Mock<IRepository<WeatherRiskAnalysis>> _mockAnalysisRepository;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();
        _mockProjectRepository = new Mock<IRepository<Project>>();
        _mockFirmRepository = new Mock<IRepository<Firm>>();
        _mockScheduleRepository = new Mock<IRepository<ProjectSchedule>>();
        _mockTaskRepository = new Mock<IRepository<WeatherGuard.Core.Entities.Task>>();
        _mockAnalysisRepository = new Mock<IRepository<WeatherRiskAnalysis>>();

        _mockUnitOfWork.Setup(u => u.Projects).Returns(_mockProjectRepository.Object);
        _mockUnitOfWork.Setup(u => u.Firms).Returns(_mockFirmRepository.Object);
        _mockUnitOfWork.Setup(u => u.ProjectSchedules).Returns(_mockScheduleRepository.Object);
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepository.Object);
        _mockUnitOfWork.Setup(u => u.WeatherRiskAnalyses).Returns(_mockAnalysisRepository.Object);

        _projectService = new ProjectService(_mockUnitOfWork.Object, _mockMapper.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_WithValidDto_ShouldReturnSuccessResult()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            Status = "Active"
        };

        var createdBy = Guid.NewGuid();
        var project = CreateTestProject();
        var projectDto = CreateTestProjectDto();

        _mockFirmRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Firm, bool>>>())).ReturnsAsync(true);
        _mockMapper.Setup(m => m.Map<Project>(createDto)).Returns(project);
        _mockProjectRepository.Setup(r => r.AddAsync(It.IsAny<Project>())).ReturnsAsync(project);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Mock the GetByIdAsync call that happens after creation
        var projects = new List<Project> { project }.AsQueryable();
        var mockSet = new Mock<DbSet<Project>>();
        mockSet.As<IAsyncEnumerable<Project>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Project>(projects.GetEnumerator()));
        mockSet.As<IQueryable<Project>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Project>(projects.Provider));
        mockSet.As<IQueryable<Project>>().Setup(m => m.Expression).Returns(projects.Expression);
        mockSet.As<IQueryable<Project>>().Setup(m => m.ElementType).Returns(projects.ElementType);
        mockSet.As<IQueryable<Project>>().Setup(m => m.GetEnumerator()).Returns(projects.GetEnumerator());

        _mockProjectRepository
            .Setup(r => r.GetQueryable(It.IsAny<System.Linq.Expressions.Expression<Func<Project, object>>[]>()))
            .Returns(projects);

        _mockMapper.Setup(m => m.Map<ProjectDto>(It.IsAny<Project>())).Returns(projectDto);
        _mockScheduleRepository.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<ProjectSchedule, bool>>>())).ReturnsAsync(0);
        _mockTaskRepository.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WeatherGuard.Core.Entities.Task, bool>>>())).ReturnsAsync(0);
        _mockAnalysisRepository.Setup(r => r.CountAsync(It.IsAny<System.Linq.Expressions.Expression<Func<WeatherRiskAnalysis, bool>>>())).ReturnsAsync(0);

        // Act
        var result = await _projectService.CreateAsync(createDto, createdBy);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        _mockProjectRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateAsync_WithNonExistingOrganization_ShouldReturnErrorResult()
    {
        // Arrange
        var createDto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project"
        };

        var createdBy = Guid.NewGuid();

        _mockFirmRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Firm, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _projectService.CreateAsync(createDto, createdBy);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Organization not found");
        result.Data.Should().BeNull();
        _mockProjectRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task ExistsAsync_WithExistingProject_ShouldReturnTrue()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>())).ReturnsAsync(true);

        // Act
        var result = await _projectService.ExistsAsync(projectId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async System.Threading.Tasks.Task ExistsAsync_WithNonExistingProject_ShouldReturnFalse()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _mockProjectRepository.Setup(r => r.ExistsAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Project, bool>>>())).ReturnsAsync(false);

        // Act
        var result = await _projectService.ExistsAsync(projectId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().BeFalse();
    }

    private Project CreateTestProject(Guid? id = null)
    {
        return new Project
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Status = "Active",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Organization = new Firm { Name = "Test Organization" },
            Creator = new User { FirstName = "John", LastName = "Doe" }
        };
    }

    private ProjectDto CreateTestProjectDto(Guid? id = null)
    {
        return new ProjectDto
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Project",
            OrganizationId = Guid.NewGuid(),
            CreatedBy = Guid.NewGuid(),
            Status = "Active",
            Description = "Test Description",
            OrganizationName = "Test Organization",
            CreatorName = "John Doe"
        };
    }
}

// Helper classes for async queryable testing
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken)
    {
        return Execute<TResult>(expression);
    }
}

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}