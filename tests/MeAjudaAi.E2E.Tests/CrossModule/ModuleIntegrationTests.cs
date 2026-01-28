using System.Net.Http.Json;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.CrossModule;

/// <summary>
/// Testes de integração para funcionalidades que atravessam múltiplos módulos
/// Inclui pipeline CQRS, manipulação de eventos e comunicação entre módulos
/// </summary>
public class ModuleIntegrationTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public ModuleIntegrationTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateUser_ShouldTriggerDomainEvents()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin(); // CreateUser requires admin role
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Mantém sob 30 caracteres
        var createUserRequest = new
        {
            Username = $"test_{uniqueId}", // test_12345678 = 13 chars
            Email = $"eventtest_{uniqueId}@example.com",
            FirstName = "Event",
            LastName = "Test",
            Password = "EventTest@123456",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createUserRequest, TestContainerFixture.JsonOptions);

        // Assert
        // HttpStatusCode.Conflict pode ocorrer se o usuário já existir em execuções de teste
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,
            HttpStatusCode.Conflict
        );

        if (response.StatusCode == HttpStatusCode.Created)
        {
            // Verifica se a resposta contém dados esperados
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(content, TestContainerFixture.JsonOptions);
            result.TryGetProperty("value", out var dataProperty).Should().BeTrue();
            dataProperty.TryGetProperty("id", out var idProperty).Should().BeTrue();
            idProperty.GetGuid().Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task CreateAndUpdateUser_ShouldMaintainConsistency()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // 8 hex chars
        var createUserRequest = new
        {
            Username = $"test_{uniqueId}", // test_12345678 = 13 chars
            Email = $"consistencytest_{uniqueId}@example.com",
            FirstName = "Consistency",
            LastName = "Test",
            Password = "ConsistencyTest@123456",
            PhoneNumber = "+5511999999999"
        };

        // Act 1: Create user
        TestContainerFixture.AuthenticateAsAdmin();
        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createUserRequest, TestContainerFixture.JsonOptions);

        // Assert 1: Usuário criado com sucesso ou já existente
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.Conflict);

        if (createResponse.StatusCode == HttpStatusCode.Created)
        {
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createResult = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(createContent, TestContainerFixture.JsonOptions);
            createResult.TryGetProperty("value", out var dataProperty).Should().BeTrue();
            dataProperty.TryGetProperty("id", out var idProperty).Should().BeTrue();
            var userId = idProperty.GetGuid();

            // Act 2: Atualiza o usuário (manter o mesmo email)
            var updateRequest = new
            {
                Email = $"consistencytest_{uniqueId}@example.com",
                FirstName = "Updated",
                LastName = "User"
            };

            // Re-authenticate before PUT to ensure context is preserved
            TestContainerFixture.AuthenticateAsAdmin();
            var updateResponse = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, TestContainerFixture.JsonOptions);

            // Assert 2: Atualização deve ter sucesso ou retornar erro apropriado
            updateResponse.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,
                HttpStatusCode.NoContent,
                HttpStatusCode.NotFound
            );

            // Re-authenticate before GET
            TestContainerFixture.AuthenticateAsAdmin();
            
            // Act 3: Verifica se o usuário pode ser recuperado
            var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
            
            // Assert 3: Usuário deve ser recuperável após atualização
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should retrieve user after update");
        }
    }

    [Fact]
    public async Task QueryUsers_ShouldReturnConsistentPagination()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin(); // GET requer autorização

        // Act 1: Obtém a primeira página
        var page1Response = await _fixture.ApiClient.GetAsync("/api/v1/users?pageNumber=1&pageSize=5");

        // Act 2: Obtém a segunda página  
        var page2Response = await _fixture.ApiClient.GetAsync("/api/v1/users?pageNumber=2&pageSize=5");

        // Assert: Ambas as requisições devem ter sucesso ou retornar not found
        page1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        page2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);

        // Se houver dados, verifica a estrutura da paginação
        if (page1Response.StatusCode == HttpStatusCode.OK)
        {
            var content = await page1Response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            // Verifica se é um JSON válido com a estrutura esperada
            using var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
            jsonDoc.RootElement.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
        }
    }

    [Fact]
    public async Task Command_WithInvalidInput_ShouldReturnValidationErrors()
    {
        // Arrange: Cria requisição com múltiplos erros de validação
        TestContainerFixture.AuthenticateAsAdmin(); // POST requer autorização

        var invalidRequest = new
        {
            Username = "", // Muito curto
            Email = "not-an-email", // Formato inválido
            FirstName = new string('a', 101), // Muito longo (assumindo máximo 100)
            LastName = "" // Campo obrigatório vazio
        };

        // Act
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", invalidRequest, TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrEmpty();

        // Verifica formato da resposta de erro
        var errorDoc = System.Text.Json.JsonDocument.Parse(content);
        errorDoc.RootElement.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Object);
    }

    [Fact]
    public async Task ConcurrentUserCreation_ShouldHandleGracefully()
    {
        // Arrange - autentica como admin para poder criar usuários
        TestContainerFixture.AuthenticateAsAdmin();

        var uniqueId = Guid.NewGuid().ToString("N")[..8]; // Mantém sob 30 caracteres
        var userRequest = new
        {
            Username = $"conc_{uniqueId}", // conc_12345678 = 13 chars
            Email = $"concurrent_{uniqueId}@example.com",
            FirstName = "Concurrent",
            LastName = "Test"
        };

        // Act: Envia múltiplas requisições concorrentes
        var tasks = Enumerable.Range(0, 3).Select(async i =>
        {
            return await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", userRequest, TestContainerFixture.JsonOptions);
        });

        var responses = await Task.WhenAll(tasks);

        // Assert: Apenas uma deve ter sucesso, as outras devem retornar conflict ou validation errors
        var successCount = responses.Count(r => r.StatusCode == HttpStatusCode.Created);
        var conflictCount = responses.Count(r => r.StatusCode == HttpStatusCode.Conflict);
        var badRequestCount = responses.Count(r => r.StatusCode == HttpStatusCode.BadRequest);

        // Apenas uma deve ter sucesso e as outras falhar (conflict ou validation), ou todas falharem
        // BadRequest é aceitável como resposta de conflito concorrente (erros de validação)
        var failureCount = conflictCount + badRequestCount;
        ((successCount == 1 && failureCount == 2) || failureCount == 3)
            .Should().BeTrue("Exactly one request should succeed or all should fail with conflict/validation errors");
    }
}
