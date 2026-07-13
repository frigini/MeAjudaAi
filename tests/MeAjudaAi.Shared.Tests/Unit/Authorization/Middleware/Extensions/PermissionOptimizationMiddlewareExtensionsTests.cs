using MeAjudaAi.Shared.Authorization.Middleware;
using MeAjudaAi.Shared.Authorization.Middleware.Extensions;
using MeAjudaAi.Shared.Authorization.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Middleware;

public class PermissionOptimizationMiddlewareExtensionsTests
{
    [Fact]
    public void GetExpectedPermissions_WithEmptyContext_ReturnsEmpty()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetExpectedPermissions();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetExpectedPermissions_WithItems_ReturnsCorrectSet()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var permissions = new[] { EPermission.UsersRead, EPermission.UsersCreate };
        context.Items[PermissionOptimizationConstants.ExpectedPermissions] = permissions;

        // Act
        var result = context.GetExpectedPermissions();

        // Assert
        result.Should().BeEquivalentTo(permissions);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldUseAggressivePermissionCache_ReturnsCorrectValue(bool value)
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Items[PermissionOptimizationConstants.UseAggressivePermissionCache] = value;

        // Act
        var result = context.ShouldUseAggressivePermissionCache();

        // Assert
        result.Should().Be(value);
    }

    [Fact]
    public void GetRecommendedPermissionCacheDuration_ReturnsDefault15Minutes()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = context.GetRecommendedPermissionCacheDuration();

        // Assert
        result.Should().Be(TimeSpan.FromMinutes(15));
    }
}
