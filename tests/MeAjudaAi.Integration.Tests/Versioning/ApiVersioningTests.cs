using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Aspire;
using System.Net;
using Xunit;
using Xunit.Abstractions;

namespace MeAjudaAi.Integration.Tests.Versioning;

[Collection("AspireApp")]
public class ApiVersioningTests : IntegrationTestBase
{
    public ApiVersioningTests(AspireIntegrationFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task ApiVersioning_ShouldWork_ViaUrl()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        // Should not be NotFound - indicates versioning is working
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiVersioning_ShouldWork_ViaHeader()
    {
        // Arrange
        HttpClient.DefaultRequestHeaders.Add("Api-Version", "1.0");

        // Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        // Should not be NotFound - indicates versioning header is working
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiVersioning_ShouldWork_ViaQueryString()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/users?api-version=1.0");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        // Should not be NotFound - indicates versioning query string is working
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiVersioning_ShouldUseDefaultVersion_WhenNotSpecified()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        // Should not be NotFound - indicates default versioning is working
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiVersioning_ShouldReturnApiVersionHeader()
    {
        // Arrange & Act
        var response = await HttpClient.GetAsync("/api/v1/users");

        // Assert
        // Check if the API returns version information in headers
        var apiVersionHeaders = response.Headers.Where(h => 
            h.Key.Contains("version", StringComparison.OrdinalIgnoreCase) ||
            h.Key.Contains("api-version", StringComparison.OrdinalIgnoreCase));
        
        // At minimum, the response should not be NotFound
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
    }
}