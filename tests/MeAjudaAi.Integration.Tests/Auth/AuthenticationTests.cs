using System.Net;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Integration.Tests.Infrastructure;

namespace MeAjudaAi.Integration.Tests.Auth;

/// <summary>
/// Testes para verificar se o sistema de autenticação mock está funcionando
/// </summary>
public class AuthenticationTests : InstanceApiTestBase
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
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.TooManyRequests);
        response.StatusCode.Should().NotBe(HttpStatusCode.OK,
            "Regular users should not have permission to access the users list endpoint");
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário admin com todas as permissões
        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert - admin deveria ter acesso à lista de usuários
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests);
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden,
            "Admin users should have permission to access the users list endpoint");
    }
}
