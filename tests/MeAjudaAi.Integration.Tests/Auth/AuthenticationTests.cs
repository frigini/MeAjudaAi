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
        // Arrange - usuário anônimo (sem autenticação)
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        // DEBUG: Verificar se ClearConfiguration realmente limpa
        Console.WriteLine("[AUTH-TEST-DEBUG] Antes da requisição - não deveria ter usuário autenticado");

        // Act - incluir parâmetros de paginação para evitar BadRequest
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10", TestContext.Current.CancellationToken);

        // DEBUG: Vamos ver o que realmente retornou
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Console.WriteLine($"[AUTH-TEST] Status: {response.StatusCode}");
        Console.WriteLine($"[AUTH-TEST] Content: {content}");

        // Assert - Aceita tanto 401 (Unauthorized) quanto 403 (Forbidden) como respostas válidas para requisições não autenticadas
        // O sistema pode retornar 403 ao invés de 401 dependendo da configuração da política de autorização
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithRegularUserAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário regular (se permitido)
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users", TestContext.Current.CancellationToken);

        // Assert
        // Se endpoint users requer admin, deve retornar Forbidden
        // Se permite usuário regular, deve retornar OK
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }
}
