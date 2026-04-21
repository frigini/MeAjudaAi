using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Database;

[Trait("Category", "Unit")]
public class SchemaPermissionsManagerTests
{
    private readonly Mock<ILogger<SchemaPermissionsManager>> _loggerMock = new();
    private readonly SchemaPermissionsManager _sut;

    public SchemaPermissionsManagerTests()
    {
        _sut = new SchemaPermissionsManager(_loggerMock.Object);
    }

    [Fact]
    public void CreateUsersModuleConnectionString_ShouldReturnCorrectFormat()
    {
        // Arrange
        var baseConn = "Host=localhost;Database=test;Password=pass";
        var rolePass = "secret'pass";

        // Act
        var result = SchemaPermissionsManager.CreateUsersModuleConnectionString(baseConn, rolePass);

        // Assert
        result.Should().Contain("Username=users_role");
        result.Should().Contain("Password=\"secret'pass\"");
        result.Should().Contain("Search Path=users,public");
    }

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("pass", null)]
    [InlineData("", "pass")]
    [InlineData("pass", " ")]
    public async Task EnsureUsersModulePermissionsAsync_WithInvalidPasswords_ShouldThrowArgumentException(string userPass, string appPass)
    {
        // Act
        var act = () => _sut.EnsureUsersModulePermissionsAsync("conn", userPass, appPass);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task AreUsersPermissionsConfiguredAsync_WhenConnectionFails_ShouldReturnFalse()
    {
        // Arrange
        var invalidConn = "Host=invalid;Database=test";

        // Act
        var result = await _sut.AreUsersPermissionsConfiguredAsync(invalidConn);

        // Assert
        result.Should().BeFalse();
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
