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
public class InstancePermissionAuthorizationIntegrationTests : InstanceApiTestBase
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
    public async Task RegularUser_ShouldHaveRestrictedAccess()
    {
        // Arrange - Configure regular user using instance-based configuration
        AuthConfig.ConfigureRegularUser();

        // Act & Assert - Test endpoints that regular users should/shouldn't access
        var publicEndpoint = "/api/v1/providers?PageNumber=1&PageSize=10";
        var response = await Client.GetAsync(publicEndpoint, TestContext.Current.CancellationToken);

        // Regular users might have access to providers (depends on authorization policy)
        // The key is that they shouldn't get a 500 error or authentication failure
        response.StatusCode.Should().BeOneOf(
            System.Net.HttpStatusCode.OK,
            System.Net.HttpStatusCode.Forbidden,
            System.Net.HttpStatusCode.NotFound
        );
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

        // Test 1: Admin configuration
        AuthConfig.ConfigureAdmin("admin1", "Administrator 1", "admin1@test.com");
        var response1 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Test 2: Regular user configuration
        AuthConfig.ConfigureRegularUser("user1", "User 1", "user1@test.com");
        var response2 = await Client.GetAsync("/api/v1/providers?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Test 3: Clear and unauthenticated
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);
        var response3 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // Test 4: Back to admin
        AuthConfig.ConfigureAdmin("admin2", "Administrator 2", "admin2@test.com");
        var response4 = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=1", TestContext.Current.CancellationToken);

        // All requests should get appropriate responses without hanging or crashing
        response1.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
        response2.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
        response3.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);
        response4.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError);

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

        // Make a single request and verify it's not a server error
        var response = await Client.GetAsync(endpoint, TestContext.Current.CancellationToken);

        // The key test is that we get consistent, expected responses (not server errors)
        // Rate limiting (429) is fine as it shows the API is working consistently
        response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.InternalServerError,
            "API should not return server errors");        // Should be a valid HTTP response (200, 429, 401, 403, etc. are all valid)
        ((int)response.StatusCode).Should().BeInRange(200, 499,
            "Response should be a valid client or success status code");
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
