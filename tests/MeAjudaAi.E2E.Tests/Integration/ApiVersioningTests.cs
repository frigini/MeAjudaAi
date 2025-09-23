using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes para validar o funcionamento do API Versioning usando URL segments
/// Pattern: /api/v{version}/module (e.g., /api/v1/users)
/// Esta abordagem é explícita, clara e evita a complexidade de múltiplos métodos de versionamento
/// </summary>
public class ApiVersioningTests : TestContainerTestBase
{
    [Fact]
    public async Task ApiVersioning_ShouldWork_ViaUrlSegment()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert
        // Should not be NotFound - indicates URL versioning is recognized and working
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        // Valid responses: 200 (OK), 401 (Unauthorized), or 400 (BadRequest with validation errors)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApiVersioning_ShouldReturnNotFound_ForInvalidPaths()
    {
        // Arrange & Act - Test paths that should NOT work without URL versioning
        var responses = new[]
        {
            await ApiClient.GetAsync("/api/users"), // No version - should be 404
            await ApiClient.GetAsync("/users"), // No api prefix - should be 404
            await ApiClient.GetAsync("/api/v2/users") // Unsupported version - should be 404 or 400
        };

        // Assert
        foreach (var response in responses)
        {
            // These paths should not be found since we only support /api/v1/
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task ApiVersioning_ShouldWork_ForDifferentModules()
    {
        // Arrange & Act - Test that versioning works for any module pattern
        var responses = new[]
        {
            await ApiClient.GetAsync("/api/v1/users"),
            // Add more modules when they exist
            // await HttpClient.GetAsync("/api/v1/services"),
            // await HttpClient.GetAsync("/api/v1/orders"),
        };

        // Assert
        foreach (var response in responses)
        {
            // Should recognize the versioned URL pattern
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }
    }
}