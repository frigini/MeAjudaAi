using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

namespace MeAjudaAi.Integration.Tests.Authorization;

/// <summary>
/// Testes de integração para o sistema de autorização baseado em permissões.
/// Valida que usuários com diferentes níveis de permissão têm acesso apropriado aos endpoints.
/// </summary>
public class PermissionAuthorizationIntegrationTests : ApiTestBase
{
    private readonly ITestOutputHelper _output;

    public PermissionAuthorizationIntegrationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task AdminUser_ShouldHaveAccessToUsersEndpoint()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RegularUser_ShouldBeForbiddenFromUsersEndpoint()
    {
        // Arrange
        AuthConfig.ConfigureRegularUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AdminUser_ShouldHaveAccessToProvidersEndpoint()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/providers", TestContext.Current.CancellationToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnauthenticatedRequest_ShouldReturnUnauthorized()
    {
        // Arrange
        AuthConfig.ClearConfiguration();

        // Act
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        LogResponseDiagnostics(response, content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private void LogResponseDiagnostics(HttpResponseMessage response, string content)
    {
        _output.WriteLine($"Response status: {response.StatusCode}");
        _output.WriteLine($"Response content length: {content.Length}");
        if (content.Length < 1000)
            _output.WriteLine($"Response content: {content}");

        _output.WriteLine("Response headers:");
        foreach (var header in response.Headers)
            _output.WriteLine($"  {header.Key}: {string.Join(", ", header.Value)}");
    }
}
