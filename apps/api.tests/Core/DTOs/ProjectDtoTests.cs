using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using WeatherGuard.Core.DTOs;

namespace WeatherGuard.Api.Tests.Core.DTOs;

public class ProjectDtoTests
{
    [Fact]
    public void CreateProjectDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            Description = "Test Description",
            Location = "New York, NY",
            Latitude = 40.7128m,
            Longitude = -74.0060m,
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            Status = "Active",
            BIMEnabled = true,
            CoordinateSystemId = "EPSG:4326",
            NorthDirection = 0,
            BuildingHeight = 100.5m,
            GrossFloorArea = 5000.0m,
            NumberOfStoreys = 10
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void CreateProjectDto_WithMissingRequiredFields_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            // Name is empty string which should fail required validation
            Name = string.Empty,
            Description = "Test Description"
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().HaveCountGreaterThan(0);
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.Name)));
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void CreateProjectDto_WithInvalidLatitude_ShouldFailValidation(decimal invalidLatitude)
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            Latitude = invalidLatitude
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.Latitude)));
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void CreateProjectDto_WithInvalidLongitude_ShouldFailValidation(decimal invalidLongitude)
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            Longitude = invalidLongitude
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.Longitude)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(361)]
    public void CreateProjectDto_WithInvalidNorthDirection_ShouldFailValidation(decimal invalidDirection)
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            NorthDirection = invalidDirection
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.NorthDirection)));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void CreateProjectDto_WithInvalidNumberOfStoreys_ShouldFailValidation(int invalidStoreys)
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = "Test Project",
            NumberOfStoreys = invalidStoreys
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.NumberOfStoreys)));
    }

    [Fact]
    public void CreateProjectDto_WithTooLongName_ShouldFailValidation()
    {
        // Arrange
        var dto = new CreateProjectDto
        {
            OrganizationId = Guid.NewGuid(),
            Name = new string('A', 256) // Exceeds 255 character limit
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().Contain(r => r.MemberNames.Contains(nameof(CreateProjectDto.Name)));
    }

    [Fact]
    public void UpdateProjectDto_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateProjectDto
        {
            Id = Guid.NewGuid(),
            Name = "Updated Project Name",
            Description = "Updated Description",
            Status = "Completed",
            BIMEnabled = false
        };

        // Act
        var validationResults = ValidateDto(dto);

        // Assert
        validationResults.Should().BeEmpty();
    }

    [Fact]
    public void ProjectDto_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var dto = new ProjectDto();

        // Assert
        dto.Name.Should().BeEmpty();
        dto.Status.Should().BeEmpty();
        dto.BIMEnabled.Should().BeFalse();
        dto.ScheduleCount.Should().Be(0);
        dto.TaskCount.Should().Be(0);
        dto.AnalysisCount.Should().Be(0);
    }

    private static List<ValidationResult> ValidateDto(object dto)
    {
        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, true);
        return results;
    }
}