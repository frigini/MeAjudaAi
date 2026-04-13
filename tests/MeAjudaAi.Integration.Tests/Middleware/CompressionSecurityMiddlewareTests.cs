using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Middleware;

/// <summary>
/// Integration tests for CompressionSecurityMiddleware
/// </summary>
public sealed class CompressionSecurityMiddlewareTests : BaseApiTest
{
    private HttpClient HttpClient => Client;

    [Fact]
    public async Task CompressionSecurity_AuthenticatedUser_ShouldDisableCompression()
    {
        // Arrange
        // CompressionSecurity middleware checks this header before UseAuthentication() executes
        // We don't need a real user in DB, just the header presence
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Health endpoint should be accessible");

        // Verify if security middleware acted
        response.Headers.Should().ContainKey("X-Compression-Disabled", 
            "Security middleware should add debug header when disabling compression");

        // CompressionSecurityMiddleware should disable compression for authenticated users
        // This prevents BREACH/CRIME attacks that exploit compression
        response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
            "Compression must be disabled for authenticated users (BREACH protection)");
        response.Content.Headers.ContentEncoding.Should().NotContain("br",
            "Brotli must be disabled for authenticated users (BREACH protection)");

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AnonymousUser_ShouldAllowCompression()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify if security middleware DID NOT act (debug header absent)
        response.Headers.Contains("X-Compression-Disabled").Should().BeFalse(
            "Security middleware should not disable compression for anonymous users");

        // For anonymous users, compression may be enabled
        // (public content is not vulnerable to BREACH)
        // Note: whether the server decides to compress or not depends on other factors
        // This test validates that middleware does NOT block compression for anonymous
        
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_AuthenticatedRequest_WithoutAcceptEncoding_ShouldSucceed()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");

        // Act
        using var response = await HttpClient.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
    }

    [Fact]
    public async Task CompressionSecurity_MultipleAuthenticatedRequests_ShouldConsistentlyDisableCompression()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act & Assert - Dispose responses immediately after use
        for (int i = 0; i < 5; i++)
        {
            using var response = await HttpClient.GetAsync("/health");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                "All authenticated requests must have compression disabled");
        }

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }

    [Fact]
    public async Task CompressionSecurity_DifferentEndpoints_ShouldApplyRulesConsistently()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer test-token");
        HttpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        var endpoints = new[] { "/health", "/api/v1/configuration/features" };

        // Act & Assert - Dispose responses immediately after use
        foreach (var endpoint in endpoints)
        {
            using var response = await HttpClient.GetAsync(endpoint);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                response.Content.Headers.ContentEncoding.Should().NotContain("gzip",
                    $"Endpoint {endpoint} should not compress for authenticated users");
            }
        }

        HttpClient.DefaultRequestHeaders.Remove("Authorization");
        HttpClient.DefaultRequestHeaders.Remove("Accept-Encoding");
    }
}
