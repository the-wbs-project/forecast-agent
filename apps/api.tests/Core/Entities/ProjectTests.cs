using FluentAssertions;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Api.Tests.Core.Entities;

public class ProjectTests
{
    [Fact]
    public void Project_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var project = new Project();

        // Assert
        project.Status.Should().Be("Active");
        project.BIMEnabled.Should().BeFalse();
        project.Tasks.Should().NotBeNull().And.BeEmpty();
        project.Schedules.Should().NotBeNull().And.BeEmpty();
        project.WeatherRiskAnalyses.Should().NotBeNull().And.BeEmpty();
        project.BIMModels.Should().NotBeNull().And.BeEmpty();
        project.CoordinationIssues.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("2024-01-01", "2024-01-31", 30)]
    [InlineData("2024-01-01", "2024-02-01", 31)]
    [InlineData("2024-01-01", "2024-01-01", 0)]
    public void DurationInDays_WithValidDates_ShouldCalculateCorrectly(string startDateStr, string endDateStr, int expectedDays)
    {
        // Arrange
        var project = new Project
        {
            StartDate = DateOnly.Parse(startDateStr),
            EndDate = DateOnly.Parse(endDateStr)
        };

        // Act
        var duration = project.DurationInDays;

        // Assert
        duration.Should().Be(expectedDays);
    }

    [Fact]
    public void DurationInDays_WithNullDates_ShouldReturnNull()
    {
        // Arrange
        var project = new Project
        {
            StartDate = null,
            EndDate = null
        };

        // Act
        var duration = project.DurationInDays;

        // Assert
        duration.Should().BeNull();
    }

    [Fact]
    public void DurationInDays_WithOnlyStartDate_ShouldReturnNull()
    {
        // Arrange
        var project = new Project
        {
            StartDate = DateOnly.Parse("2024-01-01"),
            EndDate = null
        };

        // Act
        var duration = project.DurationInDays;

        // Assert
        duration.Should().BeNull();
    }

    [Fact]
    public void Project_WithValidData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();
        var name = "Test Project";
        var description = "Test Description";
        var location = "New York, NY";
        var latitude = 40.7128m;
        var longitude = -74.0060m;

        // Act
        var project = new Project
        {
            OrganizationId = organizationId,
            Name = name,
            Description = description,
            Location = location,
            Latitude = latitude,
            Longitude = longitude,
            StartDate = DateOnly.Parse("2024-01-01"),
            EndDate = DateOnly.Parse("2024-12-31"),
            CreatedBy = createdBy,
            BIMEnabled = true,
            BuildingHeight = 100.5m,
            GrossFloorArea = 5000.0m,
            NumberOfStoreys = 10
        };

        // Assert
        project.OrganizationId.Should().Be(organizationId);
        project.Name.Should().Be(name);
        project.Description.Should().Be(description);
        project.Location.Should().Be(location);
        project.Latitude.Should().Be(latitude);
        project.Longitude.Should().Be(longitude);
        project.CreatedBy.Should().Be(createdBy);
        project.BIMEnabled.Should().BeTrue();
        project.BuildingHeight.Should().Be(100.5m);
        project.GrossFloorArea.Should().Be(5000.0m);
        project.NumberOfStoreys.Should().Be(10);
        project.DurationInDays.Should().Be(365);
    }
}