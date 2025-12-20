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
        // Limpar qualquer configuração de autenticação e desabilitar acesso não autenticado
        AuthConfig.ClearConfiguration();
        AuthConfig.SetAllowUnauthenticated(false);

        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Deve retornar 401 Unauthorized ou 403 Forbidden quando a autenticação falha
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
        // Arrange - Configurar usuário admin
        AuthConfig.ConfigureAdmin();

        // Act - Fazer uma única requisição para verificar que o middleware de rate limiting está configurado
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // Assert - Este teste documenta que rate limiting existe no pipeline
        // Comportamento real de rate limiting (429 após N requisições) deve ser testado separadamente com setup determinístico
        // Por enquanto, verificamos que o endpoint é alcançável com rate limiting habilitado
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.TooManyRequests);
    }
}
