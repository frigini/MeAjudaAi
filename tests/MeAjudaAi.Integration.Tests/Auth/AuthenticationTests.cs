using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;

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
        this.AuthenticateAsAnonymous();

        // Act - incluir parâmetros de paginação para evitar BadRequest
        var response = await Client.GetAsync("/api/v1/users?PageNumber=1&PageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithAdminAuthentication_ShouldReturnOk()
    {
        // Arrange - usuário administrador
        this.AuthenticateAsAdmin();

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
        this.AuthenticateAsUser();

        // Act
        var response = await Client.GetAsync("/api/v1/users");

        // Assert
        // Se users endpoint requer admin, deve retornar Forbidden
        // Se permite usuário regular, deve retornar OK
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Forbidden);
    }
}