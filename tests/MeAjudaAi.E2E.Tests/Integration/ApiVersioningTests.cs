using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Integration;

/// <summary>
/// Testes para validar o funcionamento do API Versioning usando segmentos de URL
/// Padrão: /api/v{version}/module (ex: /api/v1/users)
/// Essa abordagem é explícita, clara e evita a complexidade de múltiplos métodos de versionamento
/// </summary>
public class ApiVersioningTests : TestContainerTestBase
{
    [Fact]
    public async Task ApiVersioning_ShouldWork_ViaUrlSegment()
    {
        // Arrange & Act
        var response = await ApiClient.GetAsync("/api/v1/users");

        // Assert
        // Não deve ser NotFound - indica que o versionamento por URL foi reconhecido e está funcionando
        response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        // Respostas válidas: 200 (OK), 401 (Unauthorized) ou 400 (BadRequest com erros de validação)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ApiVersioning_ShouldReturnNotFound_ForInvalidPaths()
    {
        // Arrange & Act - Testa caminhos que NÃO devem funcionar sem versionamento na URL
        var responses = new[]
        {
            await ApiClient.GetAsync("/api/users"), // Sem versão - deve ser 404
            await ApiClient.GetAsync("/users"), // Sem prefixo api - deve ser 404
            await ApiClient.GetAsync("/api/v2/users") // Versão não suportada - deve ser 404 ou 400
        };

        // Assert
        foreach (var response in responses)
        {
            // Esses caminhos não devem ser encontrados pois só suportamos /api/v1/
            response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task ApiVersioning_ShouldWork_ForDifferentModules()
    {
        // Arrange - Configure authentication for API access
        AuthenticateAsAdmin();

        // Act - Testa que o versionamento funciona para diferentes padrões de módulos
        var responses = new[]
        {
            await ApiClient.GetAsync("/api/v1/users"),
            await ApiClient.GetAsync("/api/v1/providers"),
            await ApiClient.GetAsync("/api/v1/service-catalogs/services"),
            await ApiClient.GetAsync("/api/v1/service-catalogs/categories"),
            await ApiClient.GetAsync("/api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=10"),
        };

        // Assert
        foreach (var response in responses)
        {
            // Deve reconhecer o padrão de URL versionada
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized, HttpStatusCode.BadRequest);
        }
    }
}
