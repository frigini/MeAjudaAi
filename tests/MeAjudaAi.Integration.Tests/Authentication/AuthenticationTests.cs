using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Authentication;

/// <summary>
/// Testes para verificar se o sistema de autenticação mock está funcionando
/// </summary>
public class AuthenticationTests : ApiTestBase
{
    [Fact]
    public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Clear any authentication configuration and disable unauthenticated access
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Should return 401 Unauthorized or 403 Forbidden when authentication fails
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithRegularUserAuthentication_ShouldReturnForbidden()
    {
        // Arrange - usuário regular sem permissão UsersList
        AuthConfig.ConfigureRegularUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - usuário regular não deveria ter acesso à lista de usuários
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário admin com todas as permissões
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - admin deveria ter acesso à lista de usuários
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task RateLimiting_EndpointIsReachable_ShouldReturnSuccessOrRateLimited()
    {
        // Arrange - Configure admin user
        AuthConfig.ConfigureAdmin();

        // Act - Make a single request to verify rate limiting middleware is configured
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - This test documents that rate limiting exists in the pipeline
        // Actual rate limiting behavior (429 after N requests) should be tested separately with deterministic setup
        // For now, we verify the endpoint is reachable with rate limiting enabled
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests);
    }
}
