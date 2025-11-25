using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Parameterized tests for GeographicRestriction feature flag.
/// Validates behavior when feature is enabled in appsettings.Testing.json.
/// Note: Testing with disabled feature requires separate test class with different environment setup.
/// 
/// TODO: Once CI middleware registration issue is resolved, consolidate with GeographicRestrictionIntegrationTests.cs
/// to eliminate duplication. Current overlap exists because these Theory-based tests were created as an alternative
/// approach to diagnose CI failures, but GeographicRestrictionIntegrationTests.cs already covers the same scenarios
/// with passing tests.
/// </summary>
[Collection("Integration")]
public class GeographicRestrictionFeatureFlagTests : ApiTestBase
{
    // TODO: Create GitHub issue to track CI middleware registration problem.
    // Multiple tests skipped due to feature flag defaulting to disabled in CI.
    // Issue affects GeographicRestrictionMiddleware not being registered during CI test runs.
    // TODO: Re-enable when CI middleware registration issue is resolved
    // Track: Feature flag/middleware not blocking in CI despite GeographicRestriction:true
    [Fact(Skip = "CI returns 200 OK instead of 451 - middleware not blocking. Likely feature flag or middleware registration issue in CI environment.")]
    public async Task GeographicRestriction_WhenEnabled_ShouldBlockUnauthorizedCities()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        try
        {
            Client.DefaultRequestHeaders.Add(UserLocationHeader, "São Paulo|SP"); // Blocked city

            // Act
            var response = await Client.GetAsync(ProvidersEndpoint);

            // Assert - Feature enabled: should block unauthorized locations
            response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons,
                "Geographic restriction should block São Paulo when feature is enabled");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove(UserLocationHeader);
        }
    }

    // Individual tests for allowed cities provide better test isolation and clearer failure reporting
    [Theory(Skip = "CI returns 200 OK instead of expected behavior. Re-enable when middleware registration is fixed.")]
    [InlineData("Muriaé", "MG")]
    [InlineData("Itaperuna", "RJ")]
    [InlineData("Linhares", "ES")]
    public async Task GeographicRestriction_WhenEnabled_ShouldAllowConfiguredCity(string city, string state)
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        try
        {
            Client.DefaultRequestHeaders.Add(UserLocationHeader, $"{city}|{state}");

            // Act
            var response = await Client.GetAsync(ProvidersEndpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK,
                $"{city}/{state} should be allowed when it's in the configured list");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove(UserLocationHeader);
        }
    }

    [Theory(Skip = "CI returns 200 OK instead of 451. Re-enable when middleware registration is fixed.")]
    [InlineData("São Paulo", "SP")]
    [InlineData("Rio de Janeiro", "RJ")]
    public async Task GeographicRestriction_WhenEnabled_ShouldBlockUnauthorizedCity(string city, string state)
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        try
        {
            Client.DefaultRequestHeaders.Add(UserLocationHeader, $"{city}|{state}");

            // Act
            var response = await Client.GetAsync(ProvidersEndpoint);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnavailableForLegalReasons,
                $"{city}/{state} should be blocked when not in the configured list");
        }
        finally
        {
            Client.DefaultRequestHeaders.Remove(UserLocationHeader);
        }
    }

    [Fact]
    public async Task GeographicRestriction_WhenEnabled_ShouldValidateLocationHeaderFormat()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        try
        {
            // Ensure no stale header from previous tests
            Client.DefaultRequestHeaders.Remove(UserLocationHeader);

            // Act - Send request without location header
            var responseWithoutHeader = await Client.GetAsync(ProvidersEndpoint);

            // Send request with malformed location header  
            Client.DefaultRequestHeaders.Add(UserLocationHeader, "InvalidFormat");
            var responseWithMalformedHeader = await Client.GetAsync(ProvidersEndpoint);

            // Assert - Should allow when location cannot be determined (fail-open)
            responseWithoutHeader.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
                "Missing location header should fail-open (allow access)");

            responseWithMalformedHeader.StatusCode.Should().NotBe(HttpStatusCode.UnavailableForLegalReasons,
                "Malformed location header should fail-open (allow access)");
        }
        finally
        {
            // Clean up header to avoid leaking into other tests
            Client.DefaultRequestHeaders.Remove(UserLocationHeader);
        }
    }
}
