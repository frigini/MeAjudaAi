using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core.Enums;
using MeAjudaAi.Shared.Authorization.Middleware;
using MeAjudaAi.Shared.Authorization.Middleware.Extensions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Middleware;

public class PermissionOptimizationMiddlewareExtensionsTests
{
    [Fact]
    public void GetExpectedPermissions_WithEmptyContext_ReturnsEmpty()
    {
        var context = new DefaultHttpContext();
        var result = context.GetExpectedPermissions();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetExpectedPermissions_WithItems_ReturnsCorrectSet()
    {
        var context = new DefaultHttpContext();
        var permissions = new[] { EPermission.UsersRead, EPermission.UsersCreate };
        context.Items[PermissionOptimizationConstants.ExpectedPermissions] = permissions;
        
        var result = context.GetExpectedPermissions();
        result.Should().BeEquivalentTo(permissions);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldUseAggressivePermissionCache_ReturnsCorrectValue(bool value)
    {
        var context = new DefaultHttpContext();
        context.Items[PermissionOptimizationConstants.UseAggressivePermissionCache] = value;
        
        context.ShouldUseAggressivePermissionCache().Should().Be(value);
    }

    [Fact]
    public void GetRecommendedPermissionCacheDuration_ReturnsDefault15Minutes()
    {
        var context = new DefaultHttpContext();
        context.GetRecommendedPermissionCacheDuration().Should().Be(TimeSpan.FromMinutes(15));
    }
}
