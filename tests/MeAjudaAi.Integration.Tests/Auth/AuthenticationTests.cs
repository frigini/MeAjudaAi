using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Shared.Tests.Auth;

namespace MeAjudaAi.Integration.Tests.Auth;

/// <summary>
/// Testes para verificar se o sistema de autenticação mock está funcionando
/// </summary>
public class AuthenticationTests : ApiTestBase
{
    [Fact]
    public async Task GetUsers_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Clear any authentication configuration and disable unauthenticated access
        ConfigurableTestAuthenticationHandler.ClearConfiguration();
        ConfigurableTestAuthenticationHandler.SetAllowUnauthenticated(false);

        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Should return 401 Unauthorized when authentication fails
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            "Endpoint requires authentication and should return 401 when no valid authentication is provided");
    }

    [Fact]
    public async Task GetUsers_WithRegularUserAuthentication_ShouldReturnForbidden()
    {
        // Arrange - usuário regular sem permissão UsersList
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - usuário regular não deveria ter acesso à lista de usuários
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden,
            "Regular users should not have permission to access the users list endpoint");
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário admin com todas as permissões
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - admin deveria ter acesso à lista de usuários
        response.StatusCode.Should().Be(HttpStatusCode.OK,
            "Admin users should have permission to access the users list endpoint");
    }
}
