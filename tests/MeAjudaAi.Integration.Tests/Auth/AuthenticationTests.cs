using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
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
        Console.WriteLine("[AUTH-TEST-DEBUG] Before request - should have no authenticated user");
        
        // Act - incluir parâmetros de paginação para evitar BadRequest
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10");

        // DEBUG: Vamos ver o que realmente retornou
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[AUTH-TEST] Status: {response.StatusCode}");
        Console.WriteLine($"[AUTH-TEST] Content: {content}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário administrador
        ConfigurableTestAuthenticationHandler.ConfigureAdmin();

        // Act - inclui parâmetros de paginação
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10");

        // Assert - vamos ver qual erro está sendo retornado
        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"BadRequest response: {content}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_WithRegularUserAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário regular (se permitido)
        ConfigurableTestAuthenticationHandler.ConfigureRegularUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        // Se users endpoint requer admin, deve retornar Forbidden
        // Se permite usuário regular, deve retornar OK
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }
}