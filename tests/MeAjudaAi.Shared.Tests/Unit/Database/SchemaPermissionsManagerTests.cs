using MeAjudaAi.Shared.Authorization.Core.Models;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;

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

    [Theory]
    [InlineData(null, "pass")]
    [InlineData("pass", null)]
    [InlineData("", "pass")]
    [InlineData("pass", " ")]
    public async Task EnsureModulePermissionsAsync_WithInvalidPasswords_ShouldThrowArgumentException(string userPass, string appPass)
    {
        // Arrange
        var config = new ModulePermissionConfig("users", "users", "role", userPass, "appRole", appPass);

        // Act
        var act = () => _sut.EnsureModulePermissionsAsync("conn", config);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task EnsureModulePermissionsAsync_WithNonExistentScript_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var config = new ModulePermissionConfig("non-existent-module", "schema", "role", "pass", "appRole", "pass");

        // Act
        var act = () => _sut.EnsureModulePermissionsAsync("conn", config);

        // Assert
        await act.Should().ThrowAsync<FileNotFoundException>();
    }
}
