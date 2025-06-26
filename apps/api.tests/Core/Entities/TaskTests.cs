using FluentAssertions;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Api.Tests.Core.Entities;

public class TaskTests
{
    [Fact]
    public void Task_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var task = new WeatherGuard.Core.Entities.Task();

        // Assert
        task.WeatherSensitive.Should().BeFalse();
        task.TaskLevel.Should().Be(0);
        task.TaskType.Should().Be("Task");
        task.IsSummaryTask.Should().BeFalse();
        task.IsMilestone.Should().BeFalse();
        task.PercentComplete.Should().Be(0);
        task.CriticalPath.Should().BeFalse();
        task.TotalFloat.Should().Be(0);
        task.FreeFloat.Should().Be(0);
        task.ChildTasks.Should().NotBeNull().And.BeEmpty();
        task.PredecessorDependencies.Should().NotBeNull().And.BeEmpty();
        task.SuccessorDependencies.Should().NotBeNull().And.BeEmpty();
        task.RiskDetails.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("2024-01-01 08:00:00", "2024-01-05 17:00:00", 4)]
    [InlineData("2024-01-01 08:00:00", "2024-01-01 17:00:00", 0)]
    [InlineData("2024-01-01 08:00:00", "2024-01-02 08:00:00", 1)]
    public void DurationInDays_WithValidDates_ShouldCalculateCorrectly(string startDateStr, string endDateStr, int expectedDays)
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            StartDate = DateTime.Parse(startDateStr),
            EndDate = DateTime.Parse(endDateStr)
        };

        // Act
        var duration = task.DurationInDays;

        // Assert
        duration.Should().Be(expectedDays);
    }

    [Fact]
    public void DurationInDays_WithNullDates_ShouldReturnDurationProperty()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            StartDate = null,
            EndDate = null,
            Duration = 10
        };

        // Act
        var duration = task.DurationInDays;

        // Assert
        duration.Should().Be(10);
    }

    [Fact]
    public void IsOverdue_WithPastEndDateAndIncompleteTask_ShouldReturnTrue()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            EndDate = DateTime.UtcNow.AddDays(-1),
            PercentComplete = 50
        };

        // Act
        var isOverdue = task.IsOverdue;

        // Assert
        isOverdue.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_WithPastEndDateAndCompleteTask_ShouldReturnFalse()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            EndDate = DateTime.UtcNow.AddDays(-1),
            PercentComplete = 100
        };

        // Act
        var isOverdue = task.IsOverdue;

        // Assert
        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_WithFutureEndDate_ShouldReturnFalse()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            EndDate = DateTime.UtcNow.AddDays(1),
            PercentComplete = 50
        };

        // Act
        var isOverdue = task.IsOverdue;

        // Assert
        isOverdue.Should().BeFalse();
    }

    [Fact]
    public void IsInProgress_WithActualStartDateButNoEndDate_ShouldReturnTrue()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            ActualStartDate = DateTime.UtcNow.AddDays(-1),
            ActualEndDate = null
        };

        // Act
        var isInProgress = task.IsInProgress;

        // Assert
        isInProgress.Should().BeTrue();
    }

    [Fact]
    public void IsInProgress_WithBothActualDates_ShouldReturnFalse()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task
        {
            ActualStartDate = DateTime.UtcNow.AddDays(-5),
            ActualEndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var isInProgress = task.IsInProgress;

        // Assert
        isInProgress.Should().BeFalse();
    }

    [Fact]
    public void GetBIMElementIds_WithValidJson_ShouldReturnCorrectList()
    {
        // Arrange
        var elementIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var task = new WeatherGuard.Core.Entities.Task();
        task.SetBIMElementIds(elementIds);

        // Act
        var result = task.GetBIMElementIds();

        // Assert
        result.Should().BeEquivalentTo(elementIds);
    }

    [Fact]
    public void GetBIMElementIds_WithNullOrEmptyJson_ShouldReturnEmptyList()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task { BIMElementIds = null };

        // Act
        var result = task.GetBIMElementIds();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetBIMElementIds_WithInvalidJson_ShouldReturnEmptyList()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task { BIMElementIds = "invalid json" };

        // Act
        var result = task.GetBIMElementIds();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SetBIMElementIds_WithValidList_ShouldSerializeCorrectly()
    {
        // Arrange
        var elementIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var task = new WeatherGuard.Core.Entities.Task();

        // Act
        task.SetBIMElementIds(elementIds);

        // Assert
        task.BIMElementIds.Should().NotBeNullOrEmpty();
        var deserializedIds = task.GetBIMElementIds();
        deserializedIds.Should().BeEquivalentTo(elementIds);
    }

    [Fact]
    public void GetWeatherCategoriesList_WithValidJson_ShouldReturnCorrectList()
    {
        // Arrange
        var categories = new List<string> { "Rain", "Snow", "Wind" };
        var task = new WeatherGuard.Core.Entities.Task();
        task.SetWeatherCategories(categories);

        // Act
        var result = task.GetWeatherCategoriesList();

        // Assert
        result.Should().BeEquivalentTo(categories);
    }

    [Fact]
    public void GetWeatherCategoriesList_WithNullOrEmptyJson_ShouldReturnEmptyList()
    {
        // Arrange
        var task = new WeatherGuard.Core.Entities.Task { WeatherCategories = null };

        // Act
        var result = task.GetWeatherCategoriesList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void SetWeatherCategories_WithValidList_ShouldSerializeCorrectly()
    {
        // Arrange
        var categories = new List<string> { "Rain", "Snow", "Wind" };
        var task = new WeatherGuard.Core.Entities.Task();

        // Act
        task.SetWeatherCategories(categories);

        // Assert
        task.WeatherCategories.Should().NotBeNullOrEmpty();
        var deserializedCategories = task.GetWeatherCategoriesList();
        deserializedCategories.Should().BeEquivalentTo(categories);
    }
}