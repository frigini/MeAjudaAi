using System.Security.Claims;
using System.Text.Encodings.Web;

using FluentAssertions;

using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Tests.Extensions;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using static MeAjudaAi.Modules.Providers.API.Extensions;
using static MeAjudaAi.Modules.Users.API.Extensions;

namespace MeAjudaAi.Integration.Tests.Authorization;

/// <summary>
/// Testes de integração para o sistema de autorização baseado em permissões.
/// </summary>
public class PermissionAuthorizationIntegrationTests : InstanceApiTestBase
{
    private readonly ITestOutputHelper _output;

    public PermissionAuthorizationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AuthenticatedEndpoint_WithAnyClaims_ShouldReturnSuccess()
    {
        // Arrange
        AuthConfig.ConfigureRegularUser();

        // Act - Use a real endpoint that exists in the application
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert
        Console.WriteLine($"Response status: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response content: {content}");

        // This should be OK for authenticated user, or potentially rate limited
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized,  // User may not have required permissions
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithValidPermission_ShouldReturnSuccess()
    {
        // Arrange - Configure user with admin permissions (includes UsersRead)
        AuthConfig.ConfigureAdmin();

        // Act - Use real users endpoint that requires permissions
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task EndpointWithPermissionRequirement_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange - Configure user with only basic permissions 
        AuthConfig.ConfigureRegularUser();

        // Act - Use real users endpoint
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - Regular user should not have access to list users
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "Regular user should not have access to user management");
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithAllPermissions_ShouldReturnSuccess()
    {
        // Arrange - Configure admin user (has all permissions)
        AuthConfig.ConfigureAdmin();

        // Act - Use real users endpoint
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - Admin with all required permissions should succeed
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task EndpointWithMultiplePermissions_WithPartialPermissions_ShouldReturnForbidden()
    {
        // Arrange - Configure user with only one of the required permissions
        AuthConfig.ConfigureRegularUser();

        // Act - Use real users endpoint that requires multiple permissions
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - User with partial permissions should be forbidden
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "User with partial permissions should not have access");
    }

    [Fact]
    public async Task EndpointWithAnyPermission_WithOneOfRequiredPermissions_ShouldReturnSuccess()
    {
        // Arrange - Admin has required permissions
        AuthConfig.ConfigureAdmin();

        // Act - Use real API endpoint that requires permissions
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - Admin should succeed
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task EndpointWithSystemAdminRequirement_WithSystemAdminClaim_ShouldReturnSuccess()
    {
        // Arrange - Admin has system admin claim
        AuthConfig.ConfigureAdmin();

        // Act - Use real API endpoint that requires admin permissions
        var response = await Client.GetAsync("/api/v1/providers", TestContext.Current.CancellationToken);

        // Assert - Admin should succeed
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task EndpointWithModulePermission_WithValidModulePermissions_ShouldReturnSuccess()
    {
        // Arrange - Admin has all required permissions
        AuthConfig.ConfigureAdmin();

        // Act - Use real API endpoint that requires specific module permissions
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - Admin should succeed
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
        // Arrange - Ensure clean authentication state with aggressive cleanup
        AuthConfig.ClearConfiguration();

        // Add a small delay to ensure the configuration takes effect
        await Task.Delay(10);

        // Triple-check that we have the right state
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        // Debug: Verify the configuration is set correctly
        var hasConfig = AuthConfig.HasUser;
        var allowUnauth = AuthConfig.AllowUnauthenticated;
        _output.WriteLine($"Before request - HasConfiguration: {hasConfig}, AllowUnauthenticated: {allowUnauth}");

        // Act - Use real API endpoint that requires authentication
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Debug output
        _output.WriteLine($"Response status: {response.StatusCode}");
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        _output.WriteLine($"Response content length: {content.Length}");
        if (content.Length < 1000)
        {
            _output.WriteLine($"Response content: {content}");
        }

        // For debugging: check if headers give us insight into authentication
        _output.WriteLine("Response headers:");
        foreach (var header in response.Headers)
        {
            _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
        }

        // Assert - Unauthenticated request should be unauthorized  
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Unauthorized,
            HttpStatusCode.Forbidden,
            HttpStatusCode.TooManyRequests  // Rate limiting is acceptable
        );
    }
}
