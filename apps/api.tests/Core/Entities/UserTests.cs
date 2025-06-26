using FluentAssertions;
using WeatherGuard.Core.Entities;

namespace WeatherGuard.Api.Tests.Core.Entities;

public class UserTests
{
    [Fact]
    public void User_DefaultValues_ShouldBeSetCorrectly()
    {
        // Arrange & Act
        var user = new User();

        // Assert
        user.Role.Should().Be("User");
        user.IsActive.Should().BeTrue();
        user.CreatedProjects.Should().NotBeNull().And.BeEmpty();
        user.UploadedSchedules.Should().NotBeNull().And.BeEmpty();
        user.GeneratedAnalyses.Should().NotBeNull().And.BeEmpty();
        user.CreatedBIMModels.Should().NotBeNull().And.BeEmpty();
        user.AssignedIssues.Should().NotBeNull().And.BeEmpty();
        user.CreatedIssues.Should().NotBeNull().And.BeEmpty();
        user.AuditLogs.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("John", "Doe", "John Doe")]
    [InlineData("John", "", "John")]
    [InlineData("", "Doe", "Doe")]
    [InlineData("", "", "")]
    [InlineData("John", null, "John")]
    [InlineData(null, "Doe", "Doe")]
    [InlineData(null, null, "")]
    public void FullName_WithVariousInputs_ShouldReturnCorrectResult(string? firstName, string? lastName, string expectedFullName)
    {
        // Arrange
        var user = new User
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be(expectedFullName);
    }

    [Fact]
    public void User_WithValidData_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var organizationId = Guid.NewGuid();
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashedPassword123";
        var role = "Admin";

        // Act
        var user = new User
        {
            OrganizationId = organizationId,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            PasswordHash = passwordHash,
            Role = role,
            LastLoginAt = DateTime.UtcNow,
            IsActive = true
        };

        // Assert
        user.OrganizationId.Should().Be(organizationId);
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
        user.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.IsActive.Should().BeTrue();
        user.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void User_WithEmptyEmail_ShouldAllowEmptyString()
    {
        // Arrange & Act
        var user = new User
        {
            Email = string.Empty
        };

        // Assert
        user.Email.Should().BeEmpty();
    }

    [Fact]
    public void User_WithNullOptionalFields_ShouldHandleGracefully()
    {
        // Arrange & Act
        var user = new User
        {
            FirstName = null,
            LastName = null,
            PasswordHash = null,
            LastLoginAt = null
        };

        // Assert
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
        user.PasswordHash.Should().BeNull();
        user.LastLoginAt.Should().BeNull();
        user.FullName.Should().BeEmpty();
    }
}