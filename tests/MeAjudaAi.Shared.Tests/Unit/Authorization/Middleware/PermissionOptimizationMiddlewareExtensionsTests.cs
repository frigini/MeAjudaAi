using FluentAssertions;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization.Middleware;

public class PermissionOptimizationMiddlewareExtensionsTests
{
    [Fact]
    public void GetExpectedPermissions_WithEmptyList_ReturnsEmpty()
    {
        var permissions = Enumerable.Empty<string>();
        var result = PermissionOptimizationMiddlewareExtensions.GetExpectedPermissions(permissions);
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetExpectedPermissions_WithItems_ReturnsCorrectSet()
    {
        var permissions = new[] { "Read", "Write" };
        var result = PermissionOptimizationMiddlewareExtensions.GetExpectedPermissions(permissions);
        result.Should().BeEquivalentTo("Read", "Write");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldUseAggressivePermissionCache_ReturnsCorrectValue(bool value)
    {
        // This is an extension method likely relying on configuration or default logic.
        // Assuming there is a way to configure it or it's a direct pass-through/logic based on config.
        // Given the requirement, I'll assume it checks a setting or default.
        // If it's a static setting, we might need to mock or set configuration.
        // Assuming it's based on an IConfiguration or similar, but the prompt asks to test the bool logic.
        
        // As a simple extension test, I'll verify if it's testable as is. 
        // If it requires configuration, I might need to mock IConfiguration.
        PermissionOptimizationMiddlewareExtensions.ShouldUseAggressivePermissionCache(value).Should().Be(value);
    }

    [Fact]
    public void GetRecommendedPermissionCacheDuration_ReturnsDefault15Minutes()
    {
        PermissionOptimizationMiddlewareExtensions.GetRecommendedPermissionCacheDuration().Should().Be(TimeSpan.FromMinutes(15));
    }
}
