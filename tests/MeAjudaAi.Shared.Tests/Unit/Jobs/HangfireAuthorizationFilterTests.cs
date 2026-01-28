using FluentAssertions;
using Hangfire;
using Hangfire.Dashboard;
using MeAjudaAi.Shared.Jobs;
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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Development environment should allow unrestricted access");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Theory]
    [InlineData("Testing")]
    [InlineData("testing")]
    [InlineData("TESTING")]
    public void Authorize_InTestingEnvironment_ShouldAllowAccess(string environment)
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Testing environment should allow unrestricted access");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_UseDotnetEnvironmentVariable_ShouldWork()
    {
        // Arrange - Apenas DOTNET_ENVIRONMENT configurado
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext();

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("Should respect DOTNET_ENVIRONMENT when ASPNETCORE_ENVIRONMENT is not set");

        // Cleanup
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_InProductionWithoutAuthentication_ShouldDenyAccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
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

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_InProductionAuthenticatedButNotAdmin_ShouldDenyAccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
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

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_InProductionWithSystemAdminRole_ShouldAllowAccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim("is_system_admin", "true") // Claim que identifica administrador do sistema
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeTrue("SystemAdmin should have access in production");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_WithNullHttpContext_ShouldDenyAccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(null!);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Null HttpContext should deny access in production");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_WithNullUser_ShouldDenyAccess()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
        var httpContext = new DefaultHttpContext { User = null! };
        var filter = new HangfireAuthorizationFilter();
        var context = CreateDashboardContext(httpContext);

        // Act
        var result = filter.Authorize(context);

        // Assert
        result.Should().BeFalse("Null User should deny access in production");

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

    [Fact]
    public void Authorize_DefaultEnvironmentIsProduction_ShouldRequireAuth()
    {
        // Arrange - Sem variável de ambiente (default = Production)
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", null);
        
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
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", environment);
        
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

        // Cleanup
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
    }

}
