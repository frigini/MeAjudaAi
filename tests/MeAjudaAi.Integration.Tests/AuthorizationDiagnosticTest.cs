using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests;

/// <summary>
/// 🧪 TESTE DIAGNÓSTICO PARA AUTHORIZATION
/// 
/// Verifica se o problema está na configuração de autorização
/// </summary>
public class AuthorizationDiagnosticTest(ITestOutputHelper testOutput) : AuthorizationTestBase
{
    [Fact]
    public async Task Health_Check_Endpoint_Should_Work_Without_Auth()
    {
        // Arrange - Try health check which should not require auth

        // Act
        var response = await Client.GetAsync("/health");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        testOutput.WriteLine($"Health endpoint status: {response.StatusCode}");
        testOutput.WriteLine($"Health endpoint content: {content}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Users_Endpoint_Without_Auth_Should_Return_401_Not_500()
    {
        // Arrange - Clear any auth configuration and disable unauthenticated access
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(false);

        // Act
        var response = await Client.GetAsync("/api/v1/users");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        testOutput.WriteLine($"Users endpoint status: {response.StatusCode}");
        testOutput.WriteLine($"Users endpoint content: {content}");

        // If authorization is working properly, should get 401 (Unauthorized), not 500 (Internal Server Error)
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Providers_Endpoint_Without_Auth_Should_Return_401_Not_500()
    {
        // Arrange - Clear any auth configuration and disable unauthenticated access
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(false);

        // Act
        var response = await Client.GetAsync("/api/v1/providers");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        testOutput.WriteLine($"Providers endpoint status: {response.StatusCode}");
        testOutput.WriteLine($"Providers endpoint content: {content}");

        // If authorization is working properly, should get 401, not 500
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
