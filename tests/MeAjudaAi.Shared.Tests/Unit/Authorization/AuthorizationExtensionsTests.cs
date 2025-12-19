using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Authorization.Handlers;
using MeAjudaAi.Shared.Authorization.Keycloak;
using MeAjudaAi.Shared.Authorization.Metrics;
using MeAjudaAi.Shared.Authorization.Services;
using MeAjudaAi.Shared.Authorization.ValueObjects;
using MeAjudaAi.Shared.Constants;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Shared.Tests.Unit.Authorization;

/// <summary>
/// Testes unitários para AuthorizationExtensions
/// Cobertura: AddPermissionBasedAuthorization, AddKeycloakPermissionResolver, UsePermissionBasedAuthorization, etc.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Authorization")]
public class AuthorizationExtensionsTests
{
    [Fact]
    public void AddPermissionBasedAuthorization_ShouldRegisterCoreServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();

        // Act
        services.AddPermissionBasedAuthorization();

        // Assert - Verify services are registered (not resolved, to avoid dependency chain issues)
        services.Should().Contain(sd => sd.ServiceType == typeof(IPermissionService));
        services.Should().Contain(sd => sd.ServiceType == typeof(IAuthorizationHandler) && sd.ImplementationType == typeof(PermissionRequirementHandler));
    }

    [Fact]
    public void AddPermissionBasedAuthorization_ShouldRegisterMetricsService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();

        // Act
        services.AddPermissionBasedAuthorization();

        // Assert
        services.Should().Contain(sd => sd.ServiceType == typeof(IPermissionMetricsService));
    }

    [Fact]
    public void AddPermissionBasedAuthorization_ShouldRegisterHealthCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddHealthChecks();

        // Act
        services.AddPermissionBasedAuthorization();

        // Assert - Health check should be registered
        var provider = services.BuildServiceProvider();
        var healthCheckService = provider.GetService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
        healthCheckService.Should().NotBeNull();
    }

    [Fact]
    public void AddPermissionBasedAuthorization_WithConfiguration_ShouldRegisterKeycloakResolver()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();
        services.AddHealthChecks();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret",
                ["Keycloak:AdminUsername"] = "admin",
                ["Keycloak:AdminPassword"] = "admin-password"
            })
            .Build();

        // Act
        services.AddPermissionBasedAuthorization(config);

        // Assert - Verify Keycloak resolver is registered
        services.Should().Contain(sd => sd.ServiceType == typeof(IKeycloakPermissionResolver));
    }

    [Fact]
    public void AddPermissionBasedAuthorization_ShouldRegisterPoliciesForAllPermissions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorization();

        // Act
        services.AddPermissionBasedAuthorization();

        // Assert
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetService<IOptions<AuthorizationOptions>>();
        authOptions.Should().NotBeNull();

        // Verify policies are registered for key permissions
        var policy = authOptions!.Value.GetPolicy("RequirePermission:users:read");
        policy.Should().NotBeNull();
        policy!.Requirements.Should().Contain(r => r is PermissionRequirement);
    }

    [Fact]
    public void AddKeycloakPermissionResolver_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var action = () => services.AddKeycloakPermissionResolver(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddKeycloakPermissionResolver_ShouldRegisterHttpClient()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret",
                ["Keycloak:AdminUsername"] = "admin",
                ["Keycloak:AdminPassword"] = "admin-password"
            })
            .Build();

        // Act
        services.AddKeycloakPermissionResolver(config);

        // Assert
        var provider = services.BuildServiceProvider();
        var httpClientFactory = provider.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull();

        var client = httpClientFactory!.CreateClient(nameof(KeycloakPermissionResolver));
        client.Should().NotBeNull();
        client.Timeout.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public void AddKeycloakPermissionResolver_ShouldConfigureOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Keycloak:BaseUrl"] = "https://keycloak.example.com",
                ["Keycloak:Realm"] = "test-realm",
                ["Keycloak:ClientId"] = "test-client",
                ["Keycloak:ClientSecret"] = "test-secret",
                ["Keycloak:AdminUsername"] = "admin",
                ["Keycloak:AdminPassword"] = "admin-password"
            })
            .Build();

        // Act
        services.AddKeycloakPermissionResolver(config);

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<KeycloakPermissionOptions>>();
        options.Should().NotBeNull();
        options!.Value.BaseUrl.Should().Be("https://keycloak.example.com");
        options.Value.Realm.Should().Be("test-realm");
        options.Value.ClientId.Should().Be("test-client");
    }

    [Fact]
    public void UsePermissionBasedAuthorization_ShouldRegisterMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = app.UsePermissionBasedAuthorization();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void AddModulePermissionResolver_ShouldRegisterResolver()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddModulePermissionResolver<TestModulePermissionResolver>();

        // Assert
        var provider = services.BuildServiceProvider();
        var resolver = provider.GetService<IModulePermissionResolver>();
        resolver.Should().NotBeNull();
        resolver.Should().BeOfType<TestModulePermissionResolver>();
    }

    [Fact]
    public void HasPermission_WithUserHavingPermission_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.UsersRead);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithUserNotHavingPermission_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermission(EPermission.UsersCreate);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WithNullPrincipal_ShouldThrowArgumentNullException()
    {
        // Arrange
        ClaimsPrincipal? principal = null;

        // Act & Assert
        var action = () => principal!.HasPermission(EPermission.UsersRead);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HasPermissions_WithUserHavingAllPermissions_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: true);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermissions_WithUserHavingOnePermission_RequireAll_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: true);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasPermissions_WithUserHavingOnePermission_RequireAny_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersRead, EPermission.UsersCreate], requireAll: false);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasPermissions_WithUserHavingNoPermissions_RequireAny_ShouldReturnFalse()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.HasPermissions([EPermission.UsersCreate, EPermission.UsersDelete], requireAll: false);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetPermissions_WithMultiplePermissions_ShouldReturnAllPermissions()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "users:create"),
            new(AuthConstants.Claims.Permission, "users:update")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(EPermission.UsersRead);
        result.Should().Contain(EPermission.UsersCreate);
        result.Should().Contain(EPermission.UsersUpdate);
    }

    [Fact]
    public void GetPermissions_WithNoPermissions_ShouldReturnEmptyList()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetPermissions_ShouldExcludeProcessingMarker()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Permission, "users:read"),
            new(AuthConstants.Claims.Permission, "*") // Processing marker
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.GetPermissions().ToList();

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(EPermission.UsersRead);
    }

    [Fact]
    public void IsSystemAdmin_WithSystemAdminClaim_ShouldReturnTrue()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.IsSystemAdmin, "true")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSystemAdmin_WithoutSystemAdminClaim_ShouldReturnFalse()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        // Act
        var result = principal.IsSystemAdmin();

        // Assert
        result.Should().BeFalse();
    }

    // Test helper class
    private class TestModulePermissionResolver : IModulePermissionResolver
    {
        public string ModuleName => "Test";

        public Task<IReadOnlyList<EPermission>> ResolvePermissionsAsync(UserId userId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<EPermission> permissions = [EPermission.UsersRead];
            return Task.FromResult(permissions);
        }

        public bool CanResolve(EPermission permission)
        {
            return permission == EPermission.UsersRead || permission == EPermission.UsersCreate;
        }
    }
}

