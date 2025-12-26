using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Shared.Authorization;
using MeAjudaAi.Shared.Authorization.Core;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.E2E.Tests.Modules.Users;

/// <summary>
/// Testes E2E completos para o módulo Users, incluindo CRUD e validações de negócio
/// Complementa os testes de autorização existentes com validações de lifecycle e persistência
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Users")]
public class UsersEndToEndTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public UsersEndToEndTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task DeleteUser_Should_RemoveFromDatabase()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"todelete_{uniqueId}",
            Email = $"todelete_{uniqueId}@example.com",
            FirstName = "ToDelete",
            LastName = "User",
            Password = "Delete@123456",
            PhoneNumber = "+5511999999999"
        };

        // Create user - must succeed for delete test to be meaningful
        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var userId = TestContainerFixture.ExtractIdFromLocation(locationHeader);

        // Act - Delete user
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert - Deletion should return OK or NoContent
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verifica que o usuário não existe mais através da API
        var getAfterDelete = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "User should not exist after deletion");
    }

    [Fact]
    public async Task DeleteUser_NonExistent_Should_Return_NotFound()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/users/{nonExistentId}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "Deleting non-existent user should return NotFound");
    }

    [Fact]
    public async Task DeleteUser_WithoutPermission_Should_Return_ForbiddenOrUnauthorized()
    {
        // Arrange - Configure user without delete permission
        ConfigurableTestAuthenticationHandler.ClearConfiguration();

        ConfigurableTestAuthenticationHandler.ConfigureUser(
            userId: "user-no-delete-123",
            userName: "nodeleteuser",
            email: "nodelete@test.com",
            permissions: [
                EPermission.UsersRead.GetValue(),
                EPermission.UsersList.GetValue()
            ],
            isSystemAdmin: false,
            roles: []
        );

        // Use a random UUID for a non-existent user (testing authorization before existence check)
        var userId = Guid.NewGuid();

        // Act - Try to delete without permission
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/users/{userId}");

        // Assert - Should get Forbidden/Unauthorized (authorization is checked before resource existence)
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateUserProfile_Should_PersistChanges()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"toupdate_{uniqueId}",
            Email = $"toupdate_{uniqueId}@example.com",
            FirstName = "Original",
            LastName = "Name",
            Password = "Update@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty();

        var userId = TestContainerFixture.ExtractIdFromLocation(locationHeader!);

        // Act - Update profile
        var updateRequest = new
        {
            Email = $"updated_{uniqueId}@example.com", // Email é obrigatório
            FirstName = "Updated",
            LastName = "Profile"
        };

        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, TestContainerFixture.JsonOptions);

        // Assert - Update should return OK or NoContent
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Se o update retornou OK com conteúdo, verifica que contém os dados atualizados
        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            var updateContent = await updateResponse.Content.ReadAsStringAsync();
            updateContent.Should().Contain("Updated");
            updateContent.Should().Contain("Profile");
        }
        else
        {
            // Se retornou NoContent, tenta buscar o usuário para confirmar as mudanças
            var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK, "user should be found after update");
            
            var content = await getResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Updated");
            content.Should().Contain("Profile");
        }
    }

    [Fact]
    public async Task GetUserByEmail_Should_ReturnCorrectUser()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var uniqueEmail = $"byemail_{uniqueId}@example.com";

        var createRequest = new
        {
            Username = $"byemail_{uniqueId}",
            Email = uniqueEmail,
            FirstName = "ByEmail",
            LastName = "Test",
            Password = "Email@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act
        var getByEmailResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/by-email/{Uri.EscapeDataString(uniqueEmail)}");

        // Assert
        getByEmailResponse.StatusCode.Should().Be(HttpStatusCode.OK,
            "User should be found by email after creation");

        var content = await getByEmailResponse.Content.ReadAsStringAsync();
        content.Should().Contain(uniqueEmail,
            "Response should contain the queried email");
    }

    [Fact]
    public async Task CreateUser_DuplicateEmail_Should_Fail()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var sharedEmail = $"duplicate_{uniqueId}@example.com";

        var firstUserRequest = new
        {
            Username = $"first_{uniqueId}",
            Email = sharedEmail,
            FirstName = "First",
            LastName = "User",
            Password = "First@123456",
            PhoneNumber = "+5511999999999"
        };

        var firstResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", firstUserRequest, TestContainerFixture.JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - Tenta criar segundo usuário com mesmo email
        var secondUserRequest = new
        {
            Username = $"second_{uniqueId}",
            Email = sharedEmail, // Email duplicado
            FirstName = "Second",
            LastName = "User",
            Password = "Second@123456",
            PhoneNumber = "+5511999999999"
        };

        var secondResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", secondUserRequest, TestContainerFixture.JsonOptions);

        // Assert
        secondResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateUser_CompleteWorkflow_ShouldPersistChanges()
    {
        // Arrange - Criar usuário
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"update_test_{uniqueId}",
            Email = $"update_test_{uniqueId}@example.com",
            FirstName = "Original",
            LastName = "Name",
            Password = "Original@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var location = createResponse.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var userId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Act - Atualizar perfil (não alterar Email para evitar conflitos)
        var updateRequest = new
        {
            FirstName = "Updated",
            LastName = "User"
        };

        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/profile",
            updateRequest,
            TestContainerFixture.JsonOptions);

        // Assert - Update deve ser bem-sucedido
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Assert - Verificar persistência das mudanças via GET
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);
        
        var data = GetResponseData(userData);
        data.TryGetProperty("firstName", out var firstNameProp).Should().BeTrue();
        data.TryGetProperty("lastName", out var lastNameProp).Should().BeTrue();
        data.TryGetProperty("email", out var emailProp).Should().BeTrue();

        firstNameProp.GetString().Should().Be("Updated",
            "First name should be persisted after update");
        lastNameProp.GetString().Should().Be("User",
            "Last name should be persisted after update");
        emailProp.GetString().Should().Be($"update_test_{uniqueId}@example.com",
            "Email should remain unchanged when not included in update");
    }

    [Fact]
    public async Task UpdateUser_MultipleUpdates_ShouldMaintainLatestChanges()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"multi_update_{uniqueId}",
            Email = $"multi_{uniqueId}@example.com",
            FirstName = "First",
            LastName = "Version",
            Password = "Multi@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var location = createResponse.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var userId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Act - Primeira atualização
        var firstUpdate = new
        {
            Email = $"multi_{uniqueId}@example.com",
            FirstName = "Second",
            LastName = "Version"
        };
        var firstUpdateResponse = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", firstUpdate, TestContainerFixture.JsonOptions);
        
        // Assert first update succeeded
        firstUpdateResponse.IsSuccessStatusCode.Should().BeTrue("first profile update should succeed");

        // Act - Segunda atualização
        var secondUpdate = new
        {
            Email = $"multi_{uniqueId}@example.com",
            FirstName = "Third",
            LastName = "Final"
        };
        var finalUpdateResponse = await _fixture.ApiClient.PutAsJsonAsync(
            $"/api/v1/users/{userId}/profile",
            secondUpdate,
            TestContainerFixture.JsonOptions);

        // Assert
        finalUpdateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Verificar que apenas a última atualização está persistida
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should be able to retrieve updated user");
        
        var content = await getResponse.Content.ReadAsStringAsync();
        var userData = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);
        var data = GetResponseData(userData);

        data.GetProperty("firstName").GetString().Should().Be("Third");
        data.GetProperty("lastName").GetString().Should().Be("Final");
        data.GetProperty("email").GetString().Should().Be($"multi_{uniqueId}@example.com");
    }

    [Fact]
    public async Task UserWorkflow_CreateUpdateDelete_ShouldCompleteSuccessfully()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Act & Assert - CREATE
        var createRequest = new
        {
            Username = $"workflow_{uniqueId}",
            Email = $"workflow_{uniqueId}@example.com",
            FirstName = "Workflow",
            LastName = "Test",
            Password = "Workflow@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        createResponse.Headers.Location.Should().NotBeNull();
        var userId = TestContainerFixture.ExtractIdFromLocation(createResponse.Headers.Location!.ToString());

        // Act & Assert - READ
        var getResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act & Assert - UPDATE
        var updateRequest = new
        {
            Email = $"workflow_{uniqueId}@example.com",
            FirstName = "Updated",
            LastName = "Workflow"
        };
        var updateResponse = await _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", updateRequest, TestContainerFixture.JsonOptions);
        updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Verify UPDATE persisted
        var verifyUpdateResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        verifyUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should retrieve user after update");
        
        var updateContent = await verifyUpdateResponse.Content.ReadAsStringAsync();
        var updatedData = JsonSerializer.Deserialize<JsonElement>(updateContent, TestContainerFixture.JsonOptions);
        var data = GetResponseData(updatedData);
        data.GetProperty("firstName").GetString().Should().Be("Updated");

        // Act & Assert - DELETE
        var deleteResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/users/{userId}");
        deleteResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Verify DELETE worked
        var verifyDeleteResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        verifyDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #region Concurrency and Conflict Validation (409)

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_Should_Return_Conflict()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var duplicateEmail = $"duplicate_{uniqueId}@example.com";

        var firstRequest = new
        {
            Username = $"first_{uniqueId}",
            Email = duplicateEmail,
            FirstName = "First",
            LastName = "User",
            Password = "Pass@123456",
            PhoneNumber = "+5511999999999"
        };

        var secondRequest = new
        {
            Username = $"second_{uniqueId}",
            Email = duplicateEmail, // MESMO email
            FirstName = "Second",
            LastName = "User",
            Password = "Pass@123456",
            PhoneNumber = "+5511999999999"
        };

        // Act - criar primeiro usuário
        var firstResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", firstRequest, TestContainerFixture.JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Act - tentar criar segundo usuário com mesmo email
        var secondResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", secondRequest, TestContainerFixture.JsonOptions);

        // Assert - deve retornar Conflict ou BadRequest
        secondResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,  // 409 - ideal para duplicata
            HttpStatusCode.BadRequest // 400 - aceitável se validação catch primeiro
        );

        if (secondResponse.StatusCode == HttpStatusCode.Conflict)
        {
            var content = await secondResponse.Content.ReadAsStringAsync();
            content.Should().Contain("email", "Mensagem de erro deve mencionar email duplicado");
        }
    }

    [Fact]
    public async Task CreateUser_WithDuplicateUsername_Should_Return_Conflict()
    {
        // Arrange
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var duplicateUsername = $"duplicate_{uniqueId}";

        var firstRequest = new
        {
            Username = duplicateUsername,
            Email = $"first_{uniqueId}@example.com",
            FirstName = "First",
            LastName = "User",
            Password = "Pass@123456",
            PhoneNumber = "+5511999999999"
        };

        var secondRequest = new
        {
            Username = duplicateUsername, // MESMO username
            Email = $"second_{uniqueId}@example.com",
            FirstName = "Second",
            LastName = "User",
            Password = "Pass@123456",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var firstResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", firstRequest, TestContainerFixture.JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var secondResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", secondRequest, TestContainerFixture.JsonOptions);

        // Assert
        secondResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,
            HttpStatusCode.BadRequest
        );
    }

    [Fact]
    public async Task UpdateUser_ConcurrentUpdates_Should_HandleGracefully()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            Username = $"concurrent_{uniqueId}",
            Email = $"concurrent_{uniqueId}@example.com",
            FirstName = "Concurrent",
            LastName = "User",
            Password = "Pass@123456",
            PhoneNumber = "+5511999999999"
        };

        var createResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/users", createRequest, TestContainerFixture.JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var location = createResponse.Headers.Location?.ToString();
        location.Should().NotBeNullOrEmpty();
        var userId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Act - disparar duas atualizações simultâneas
        var update1 = new { Email = $"concurrent_{uniqueId}@example.com", FirstName = "Update1", LastName = "User" };
        var update2 = new { Email = $"concurrent_{uniqueId}_v2@example.com", FirstName = "Update2", LastName = "User" };

        var task1 = _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", update1, TestContainerFixture.JsonOptions);
        var task2 = _fixture.ApiClient.PutAsJsonAsync($"/api/v1/users/{userId}/profile", update2, TestContainerFixture.JsonOptions);

        var responses = await Task.WhenAll(task1, task2);

        // Assert - pelo menos uma deve ter sucesso
        responses.Should().Contain(r => r.IsSuccessStatusCode, "Pelo menos uma atualização concorrente deve suceder");

        // Verificar estado final
        var finalResponse = await _fixture.ApiClient.GetAsync($"/api/v1/users/{userId}");
        finalResponse.StatusCode.Should().Be(HttpStatusCode.OK, "should retrieve user after concurrent updates");
        
        var content = await finalResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Update", "Estado final deve refletir uma das atualizações");
    }

    #endregion

    /// <summary>
    /// Método auxiliar para extrair dados de uma resposta da API que pode estar encapsulada ou não
    /// </summary>
    private static JsonElement GetResponseData(JsonElement response)
    {
        // Se a resposta tem uma propriedade 'data', desencapsula ela
        if (response.TryGetProperty("data", out var data))
        {
            return data;
        }
        // Caso contrário, retorna a resposta diretamente
        return response;
    }
}


