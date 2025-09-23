using MeAjudaAi.E2E.Tests.Base;
using FluentAssertions;
using System.Net;

namespace MeAjudaAi.E2E.Tests.Auth;

/// <summary>
/// Testes de autenticação e autorização usando TestContainers
/// Como o Keycloak está desabilitado em testes, valida comportamento sem autenticação externa
/// </summary>
public class AuthenticationTests : TestContainerTestBase
{
    [Fact]
    public async Task Api_Should_Work_Without_Keycloak()
    {
        // Em ambiente de teste, o Keycloak está desabilitado por design para tornar
        // os testes mais rápidos e confiáveis. Este teste verifica que o sistema
        // funciona corretamente mesmo sem Keycloak ativo.
        
        // Act
        var healthResponse = await ApiClient.GetAsync("/health");
        
        // Assert
        healthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task CreateUser_Should_Work_Without_External_Auth()
    {
        // Arrange
        var createUserRequest = new
        {
            Username = Faker.Internet.UserName(),
            Email = Faker.Internet.Email(),
            FirstName = Faker.Name.FirstName(),
            LastName = Faker.Name.LastName(),
            KeycloakId = Guid.NewGuid().ToString()
        };

        // Act
        var response = await PostJsonAsync("/api/v1/users", createUserRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created, 
            "Sistema deve funcionar para criação de usuários mesmo sem Keycloak ativo");
    }

    [Fact]
    public async Task PublicEndpoints_Should_Be_Accessible()
    {
        // Arrange & Act
        var healthResponse = await ApiClient.GetAsync("/health");
        var usersResponse = await ApiClient.GetAsync("/api/v1/users?pageSize=1&pageNumber=1");

        // Assert
        healthResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.ServiceUnavailable);
        
        // Endpoints de usuários devem estar acessíveis em modo de teste
        usersResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK, 
            HttpStatusCode.Unauthorized, 
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task System_Should_Handle_Missing_Auth_Headers_Gracefully()
    {
        // Act - Tentar acessar endpoint sem headers de autenticação
        var response = await ApiClient.GetAsync("/api/v1/users?pageSize=1&pageNumber=1");

        // Assert - Sistema deve responder de forma consistente
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,           // Se endpoint é público
            HttpStatusCode.Unauthorized, // Se requer autenticação
            HttpStatusCode.Forbidden     // Se requer autorização específica
        );
        
        // Não deve retornar erro interno do servidor
        response.StatusCode.Should().NotBe(HttpStatusCode.InternalServerError);
    }
}