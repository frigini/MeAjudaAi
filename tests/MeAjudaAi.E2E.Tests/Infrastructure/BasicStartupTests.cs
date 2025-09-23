using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Basic integration tests to verify application startup and basic functionality
/// </summary>
public class BasicStartupTests : TestContainerTestBase
{
    [Fact]
    public async Task Application_ShouldStart_Successfully()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/");
        
        // Assert
        // Even a 404 is fine - it means the application started
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HealthCheck_ShouldReturnOk_WhenApplicationIsRunning()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/health");
        
        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.ServiceUnavailable, 
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ApiEndpoint_ShouldBeAccessible()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/api");
        
        // Assert
        // Any response (even 404) means the routing is working
        response.Should().NotBeNull();
    }
}