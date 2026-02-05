using FluentAssertions;
using Hangfire;
using Hangfire.Dashboard;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Shared.Tests.Unit.Jobs;

/// <summary>
/// Testes unitários para HangfireAuthorizationFilter
/// Verifica ACL do dashboard Hangfire por ambiente e role
/// </summary>
[Collection("EnvironmentVariableTests")]
[Trait("Category", "Unit")]
[Trait("Component", "Hangfire")]
public class HangfireAuthorizationFilterTests
{
    private static AspNetCoreDashboardContext CreateDashboardContext(HttpContext? httpContext = null)
    {
        var context = httpContext ?? new DefaultHttpContext();
        
        // AspNetCoreDashboardContext requires an IServiceProvider
        if (context.RequestServices == null)
        {
            var services = new ServiceCollection();
            context.RequestServices = services.BuildServiceProvider();
        }
        
        return new AspNetCoreDashboardContext(
            new Hangfire.InMemory.InMemoryStorage(),
            new DashboardOptions(),
            context);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("development")]
    [InlineData("DEVELOPMENT")]
    public void Authorize_InDevelopmentEnvironment_ShouldAllowAccess(string environment)
    {
        // Arrange
        using var _ = new EnvironmentScope(environment, null);
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Development environment should allow unrestricted access");
    }

    [Fact]
    public void Authorize_InProductionEnvironment_WithAdminRole_ShouldAllowAccess()
    {
        // Arrange
        using var _ = new EnvironmentScope("Production", null);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(AuthConstants.Claims.IsSystemAdmin, "true") // Claim expected by the filter for admin access
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        
        var context = CreateDashboardContext(httpContext);
        var filter = new HangfireAuthorizationFilter();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Authenticated admin users should have access in Production");
    }

    [Theory]
    [InlineData("Testing")]
    [InlineData("testing")]
    [InlineData("TESTING")]
    public void Authorize_InTestingEnvironment_ShouldAllowAccess(string environment)
    {
        // Arrange
        using var _ = new EnvironmentScope(environment, null);
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Testing environment should allow unrestricted access");
    }

    [Fact]

    public void Authorize_UseDotnetEnvironmentVariable_ShouldWork()
    {
        // Arrange
        using var _ = new EnvironmentScope(null, "Development");
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Should respect DOTNET_ENVIRONMENT when ASPNETCORE_ENVIRONMENT is not set");
    }

    [Fact]
    public void Authorize_InProductionWithoutAuthentication_ShouldDenyAccess()
    {
        // Arrange
        using var _ = new EnvironmentScope("Production", null);
        
        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(false);
        
        var principal = new ClaimsPrincipal(identity.Object);
        var httpContext = new DefaultHttpContext { User = principal };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Production requires authentication");
    }

    [Fact]
    public void Authorize_InProductionAuthenticatedButNotAdmin_ShouldDenyAccess()
    {
        // Arrange
        using var _ = new EnvironmentScope("Production", null);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "regular-user"),
            new Claim(ClaimTypes.Role, "User") // Role regular, não admin
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Production requires SystemAdmin role");
    }

    [Fact]
    public void Authorize_WithDefaultHttpContext_ShouldDenyAccess()
    {
        // Arrange
        using var _ = new EnvironmentScope("Production", null);
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(null!);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Default HttpContext should deny access in production");
    }

    private readonly struct EnvironmentScope : IDisposable
    {
        private readonly string? _originalAspNetCore;
        private readonly string? _originalDotNet;

        public EnvironmentScope(string? aspNetCore, string? dotNet)
        {
            _originalAspNetCore = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _originalDotNet = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", aspNetCore);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", dotNet);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", _originalAspNetCore);
            Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", _originalDotNet);
        }
    }

    [Fact]

    public void Authorize_WithNullUser_ShouldDenyAccess()
    {
        // Arrange
        using var _ = new EnvironmentScope("Production", null);
        var httpContext = new DefaultHttpContext { User = null! };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Null User should deny access in production");
    }

    [Fact]

    public void Authorize_DefaultEnvironmentIsProduction_ShouldRequireAuth()
    {
        // Arrange
        using var _ = new EnvironmentScope(null, null);
        
        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(false);
        var principal = new ClaimsPrincipal(identity.Object);
        var httpContext = new DefaultHttpContext { User = principal };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Default environment (Production) should require authentication");
    }

    [Theory]
    [InlineData("Staging")]
    [InlineData("PreProduction")]
    [InlineData("QA")]

    public void Authorize_InNonDevelopmentEnvironments_ShouldRequireAuth(string environment)
    {
        // Arrange
        using var _ = new EnvironmentScope(environment, null);
        
        var identity = new Mock<ClaimsIdentity>();
        identity.Setup(i => i.IsAuthenticated).Returns(false);
        var principal = new ClaimsPrincipal(identity.Object);
        
        var httpContext = new DefaultHttpContext { User = principal };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse($"{environment} environment should require authentication like Production");
    }

}

[CollectionDefinition("EnvironmentVariableTests", DisableParallelization = true)]
public class EnvironmentVariableTestsCollection { }
