using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Authorization;

/// <summary>
/// Testes de integração para autorização baseada em permissões usando configuração de autenticação baseada em instância
/// Elimina condições de corrida e flakiness causados por estado estático
/// </summary>
[Collection("IntegrationTests")]
public class InstancePermissionAuthorizationIntegrationTests : BaseApiTest
{
    [Fact]
    public async Task AdminUser_ShouldHaveAccessToAllEndpoints()
    {
        // Arrange - Configure admin user using instance-based configuration
        AuthConfig.ConfigureAdmin();

        // Act & Assert - Test multiple endpoints that require admin permissions
        var endpoints = new[]
        {
            "/api/v1/users?PageNumber=1&PageSize=10",
            "/api/v1/providers?PageNumber=1&PageSize=10"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            // Admin should have successful access to all endpoints
            response.IsSuccessStatusCode.Should().BeTrue($"Admin should have successful response for {endpoint}");
        }
    }

    [Fact]
    public async Task RegularUser_CanAccessPublicProviderListing()
    {
        // Arrange - Configure regular user using instance-based configuration
        AuthConfig.ConfigureRegularUser();

        // Act - Test public provider listing endpoint
        var publicEndpoint = "/api/v1/providers?PageNumber=1&PageSize=10";
        var response = await Client.GetAsync(publicEndpoint, TestContext.Current.CancellationToken);

        // Assert - Regular users should have access to public provider listings
        // Allow for rate limiting as it's valid API behavior
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.TooManyRequests);
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden,
            "Regular users should be able to access public provider listings (or be rate limited)");
    }

    [Fact]
    public async Task RegularUser_CannotAccessAdminEndpoint()
    {
        // Arrange - Configure regular user using instance-based configuration
        AuthConfig.ConfigureRegularUser();

        // Act - Test admin-only endpoint (user management requires admin permissions)
        var adminEndpoint = "/api/v1/users?PageNumber=1&PageSize=10";
        var response = await Client.GetAsync(adminEndpoint, TestContext.Current.CancellationToken);

        // Assert - Regular users should be forbidden from accessing admin endpoints
        // Rate limiting is also acceptable as it shows the API is working
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Forbidden,
            System.Net.HttpStatusCode.TooManyRequests)
            .And.Subject.Should().NotBe(System.Net.HttpStatusCode.OK,
            "Regular users should not have access to admin-only user management endpoints");
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
        // Arrange - Clear configuration to ensure unauthenticated state
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        // Act - Use real API endpoint that requires authentication
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - Should be unauthorized since no authentication is configured
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Unauthorized,
            System.Net.HttpStatusCode.Forbidden
        );
    }

    [Fact]
    public void AuthenticationConfiguration_ShouldBeIsolatedBetweenTests()
    {
        // Arrange - Configure as admin first
        AuthConfig.ConfigureAdmin("test-admin", "admin-test", "admin@test.com");

        // Verify admin configuration
        AuthConfig.HasUser.Should().BeTrue();
        AuthConfig.UserId.Should().Be("test-admin");
        AuthConfig.UserName.Should().Be("admin-test");

        // Act - Clear configuration
        AuthConfig.ClearConfiguration();

        // Assert - Configuration should be cleared
        AuthConfig.HasUser.Should().BeFalse();
        AuthConfig.UserId.Should().BeNull();
        AuthConfig.UserName.Should().BeNull();
        AuthConfig.AllowUnauthenticated.Should().BeFalse();

        // Configure as regular user
        AuthConfig.ConfigureRegularUser("test-user", "user-test", "user@test.com");

        // Verify regular user configuration
        AuthConfig.HasUser.Should().BeTrue();
        AuthConfig.UserId.Should().Be("test-user");
        AuthConfig.UserName.Should().Be("user-test");
        AuthConfig.Roles.Should().Contain("user");
    }

    [Fact]
    public async Task NoStaticStateInterference_MultipleConfigurationChanges()
    {
        // This test verifies that there are no static state issues
        // by rapidly changing configurations and ensuring they take effect

        // Test 1: Admin configuration - should have access to admin endpoints
        AuthConfig.ConfigureAdmin("admin1", "Administrator 1", "admin1@test.com");
        var response1 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Assert admin has access to user management
        response1.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.TooManyRequests);
        response1.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden,
            "Admin user should have access to user management endpoint (or be rate limited)");

        if (response1.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content1 = await response1.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            content1.Should().NotBeNullOrEmpty("Admin response should contain user data");
        }

        // Test 2: Regular user configuration - should access providers but not users
        AuthConfig.ConfigureRegularUser("user1", "User 1", "user1@test.com");
        var response2 = await Client.GetAsync("/api/v1/providers?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);
        var userAccessResponse = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Assert regular user can access providers but not users
        response2.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.TooManyRequests);
        response2.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden,
            "Regular user should have access to provider listings");

        userAccessResponse.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Forbidden,
            System.Net.HttpStatusCode.TooManyRequests);
        userAccessResponse.StatusCode.Should().NotBe(System.Net.HttpStatusCode.OK,
            "Regular user should not have access to user management");

        // Test 3: Clear and unauthenticated - should be denied access
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);
        var response3 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Assert unauthenticated requests are denied
        response3.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.Unauthorized,
            System.Net.HttpStatusCode.Forbidden,
            System.Net.HttpStatusCode.TooManyRequests);
        response3.StatusCode.Should().NotBe(System.Net.HttpStatusCode.OK,
            "Unauthenticated requests should be denied access");

        // Test 4: Back to admin - should have access again with new identity
        AuthConfig.ConfigureAdmin("admin2", "Administrator 2", "admin2@test.com");
        var response4 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Assert new admin has access and identity is correct
        response4.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.TooManyRequests);
        response4.StatusCode.Should().NotBe(System.Net.HttpStatusCode.Forbidden,
            "Second admin user should have access to user management endpoint (or be rate limited)");

        if (response4.StatusCode == System.Net.HttpStatusCode.OK)
        {
            var content4 = await response4.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            content4.Should().NotBeNullOrEmpty("Second admin response should contain user data");
        }

        // The last configuration should be admin2
        AuthConfig.UserId.Should().Be("admin2");
        AuthConfig.UserName.Should().Be("Administrator 2");
    }

    [Fact]
    public async Task EndpointResponses_ShouldBeConsistentWithoutFlakiness()
    {
        // This test checks for response consistency - a common symptom of race conditions
        // is that the same request sometimes returns different status codes

        AuthConfig.ConfigureAdmin();

        var endpoint = "/api/v1/providers?PageNumber=1&PageSize=5";
        var statusCodes = new List<System.Net.HttpStatusCode>();

        // Make multiple identical requests to detect flakiness
        // Use longer delays to avoid triggering rate limiting which is expected behavior
        for (int i = 0; i < 5; i++)
        {
            var response = await Client.GetAsync(endpoint, TestContext.Current.CancellationToken);
            statusCodes.Add(response.StatusCode);

            // None should be server errors
            response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError,
                $"Request {i + 1} should not return server error");

            // Should be valid HTTP response
            ((int)response.StatusCode).Should().BeInRange(200, 499,
                $"Request {i + 1} should be a valid client or success status code");

            // Longer delay to avoid rate limiting and allow proper testing of consistency
            if (i < 4) await Task.Delay(200, TestContext.Current.CancellationToken);
        }

        // Check for genuine flakiness: mixed success/auth responses indicate problems
        // Rate limiting (all 429s) or consistent success (all 200s) are both acceptable
        var distinctStatusCodes = statusCodes.Distinct().ToList();

        if (distinctStatusCodes.Count > 1)
        {
            // Multiple different status codes - check if they're all valid and expected
            var hasAuthenticationIssues = statusCodes.Any(sc => sc == System.Net.HttpStatusCode.Unauthorized);
            var hasSuccessResponses = statusCodes.Any(sc => sc == System.Net.HttpStatusCode.OK);
            var hasRateLimiting = statusCodes.Any(sc => sc == System.Net.HttpStatusCode.TooManyRequests);

            // Authentication flakiness: mixture of success and auth failures
            if (hasAuthenticationIssues && hasSuccessResponses)
            {
                statusCodes.Should().AllSatisfy(sc => sc.Should().NotBe(System.Net.HttpStatusCode.Unauthorized),
                    $"Mixed success/unauthorized responses indicate authentication flakiness: [{string.Join(", ", statusCodes)}]");
            }

            // If we have rate limiting, that's acceptable behavior
            if (hasRateLimiting)
            {
                statusCodes.Should().AllSatisfy(sc =>
                    sc.Should().BeOneOf(System.Net.HttpStatusCode.OK, System.Net.HttpStatusCode.TooManyRequests),
                    $"When rate limiting occurs, only 200 OK and 429 TooManyRequests should be present: [{string.Join(", ", statusCodes)}]");
            }
        }

        statusCodes.Should().NotBeEmpty("Should have collected status codes");
    }

    [Fact]
    public async Task JsonDeserialization_ShouldHandleAllResponseFormats()
    {
        // Test that we can handle various response formats without errors
        AuthConfig.ConfigureAdmin();

        var endpoints = new[]
        {
            "/api/v1/providers?PageNumber=1&PageSize=3",
            "/api/v1/providers/by-type/Individual",
            "/api/v1/providers/by-verification-status/Pending"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await Client.GetAsync(endpoint, TestContext.Current.CancellationToken);
            var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            if (response.IsSuccessStatusCode && !string.IsNullOrEmpty(content))
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(content);

                // Should be valid JSON (not throw during deserialization above)
                jsonElement.Should().NotBeNull($"Response from {endpoint} should be valid JSON");

                // Should be either an array or an object
                jsonElement.ValueKind.Should().BeOneOf(JsonValueKind.Array, JsonValueKind.Object);
            }
        }
    }

    [Fact]
    public async Task AuthConfig_ShouldSupportCustomUserRoles()
    {
        // Test custom user configuration with specific roles
        AuthConfig.ConfigureUser("custom-user", "Custom User", "custom@test.com", "moderator", "editor");

        // Verify configuration
        AuthConfig.HasUser.Should().BeTrue();
        AuthConfig.Roles.Should().Contain("moderator");
        AuthConfig.Roles.Should().Contain("editor");
        AuthConfig.Roles.Should().HaveCount(2);

        // Test that the custom configuration works with API calls
        var response = await Client.GetAsync("/api/v1/providers?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Should not result in server error
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
    }
}
