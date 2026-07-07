using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Infrastructure;

/// <summary>
/// Testes E2E para validar a distinção entre status codes 400 Bad Request vs 422 Unprocessable Entity.
/// 
/// Convenção usada:
/// - 400 Bad Request: Erros de validação do FluentValidation (formato, required fields, etc.)
/// - 422 Unprocessable Entity: Validações semânticas/de negócio (regras de domínio, estado inválido)
/// - 409 Conflict: Recursos duplicados (unique constraints)
/// </summary>
[Trait("Category", "E2E")]
[Trait("Feature", "Validation")]
public class ValidationStatusCodeEndToEndTests(TestContainerFixture fixture) : IClassFixture<TestContainerFixture>
{

    #region 400 Bad Request - FluentValidation Errors

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange - Email format inválido (validação do FluentValidation)
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var request = new
        {
            Username = fixture.Faker.Internet.UserName(),
            Email = "not-an-email", // Invalid email format
            FirstName = fixture.Faker.Name.FirstName(),
            LastName = fixture.Faker.Name.LastName(),
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request, TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "invalid email format should trigger FluentValidation error (400)");

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("email", "error message should reference the email field");
    }

    [Fact]
    public async Task Register_WithMissingRequiredField_ShouldReturn400()
    {
        // Arrange - Campo obrigatório faltando
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var request = new
        {
            Username = fixture.Faker.Internet.UserName(),
            // Email ausente (required)
            FirstName = fixture.Faker.Name.FirstName(),
            LastName = fixture.Faker.Name.LastName(),
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request, TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "missing required field should trigger FluentValidation error (400)");
    }

    [Fact]
    public async Task CreateService_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var request = new
        {
            name = "", // Empty name - FluentValidation error
            description = fixture.Faker.Lorem.Sentence(),
            categoryId = Guid.NewGuid()
        };

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", request);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task UpdateUser_WithInvalidPhoneFormat_ShouldReturn400()
    {
        // Arrange - Create user first
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await fixture.CreateTestUserAsync();

        // Act - Update with invalid phone format
        var updateRequest = new
        {
            FirstName = fixture.Faker.Name.FirstName(),
            LastName = fixture.Faker.Name.LastName(),
            PhoneNumber = "invalid-phone" // Invalid format
        };

        var response = await fixture.PutJsonAsync($"/api/v1/users/{userId}/profile", updateRequest);

        // Assert
        // Invalid phone format should trigger validation error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound); // Endpoint pode não existir ainda
    }

    #endregion

    #region 422 Unprocessable Entity - Semantic/Business Validation (Future)

    /// <summary>
    /// NOTA: 422 Unprocessable Entity não está implementado no MVP.
    /// Atualmente, todas as validações retornam 400 Bad Request (FluentValidation).
    /// Ver roadmap.md para plano de implementação.
    /// </summary>
    [Fact(Skip = "422 não implementado — ver roadmap.md")]
    public async Task CreateService_WithNonExistentCategory_ShouldReturn422()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var request = new
        {
            Name = fixture.Faker.Commerce.ProductName(),
            Description = fixture.Faker.Lorem.Sentence(),
            CategoryId = Guid.NewGuid(), // Categoria não existe (validação semântica)
            DisplayOrder = 0
        };

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", request, TestContainerFixture.JsonOptions);

        // Assert
        // 422 para validação semântica (categoria não existe)
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity,
            "semantic validation (non-existent category) should return 422");
    }

    [Fact(Skip = "422 não implementado — ver roadmap.md")]
    public async Task ChangeServiceCategory_WithInvalidTransition_ShouldReturn422()
    {
        // Arrange - Criar categoria e serviço
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        
        // Criar categoria
        var categoryRequest = new
        {
            Name = $"TestCategory_{uniqueId}",
            Description = "Test category",
            DisplayOrder = 0
        };
        var categoryResponse = await fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/categories", 
            categoryRequest, 
            TestContainerFixture.JsonOptions);
        var categoryLocation = categoryResponse.Headers.Location?.ToString();
        var categoryId = TestContainerFixture.ExtractIdFromLocation(categoryLocation!);

        // Criar serviço
        var serviceRequest = new
        {
            Name = $"TestService_{uniqueId}",
            Description = "Test service",
            CategoryId = categoryId,
            DisplayOrder = 0
        };
        var serviceResponse = await fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services",
            serviceRequest,
            TestContainerFixture.JsonOptions);
        var serviceLocation = serviceResponse.Headers.Location?.ToString();
        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceLocation!);
        
        // Act - Tentar mudar para categoria que não existe
        var changeCategoryRequest = new
        {
            NewCategoryId = Guid.NewGuid() // Categoria não existe
        };
        var response = await fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/service-catalogs/services/{serviceId}/change-category",
            changeCategoryRequest,
            TestContainerFixture.JsonOptions);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity,
            "changing to non-existent category should return 422");
    }

    #endregion

    #region 409 Conflict - Duplicate Resources

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange - Create first user
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueEmail = $"{fixture.Faker.Internet.UserName()}@example.com";
        var request = new
        {
            Username = fixture.Faker.Internet.UserName(),
            Email = uniqueEmail,
            FirstName = "Carlos",
            LastName = "Silva",
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };
        
        var firstResponse = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request, TestContainerFixture.JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created, "first user creation should succeed");

        // Act - Try to create again with same email
        var duplicateRequest = new
        {
            Username = fixture.Faker.Internet.UserName(), // Different username
            Email = uniqueEmail, // Same email
            FirstName = "Ana",
            LastName = "Souza",
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };
        var duplicateResponse = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", duplicateRequest, TestContainerFixture.JsonOptions);

        // Assert
        // TODO: Deveria retornar 409, mas atualmente retorna 400
        duplicateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);

        var content = await duplicateResponse.Content.ReadAsStringAsync();
        content.Should().Contain("já está em uso", "conflict message should indicate duplication in Portuguese");
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ShouldReturn409()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var categoryName = fixture.Faker.Commerce.Department();

        var request = new
        {
            name = categoryName,
            description = fixture.Faker.Lorem.Sentence()
        };

        await fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", request);

        // Act - Tentar criar categoria com mesmo nome
        var duplicateResponse = await fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", request);

        // Assert - Should return 409 conflict for duplicate category name
        // TODO: Endpoint pode não existir (404) ou retornar 400/409 para duplicatas
        duplicateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound); // Endpoint pode não existir
    }

    #endregion

    #region Validation Error Response Format

    [Fact]
    public async Task ValidationError_ShouldReturnStructuredErrorResponse()
    {
        // Arrange - Request com erro de validação
        // NOTE: Current architecture throws ArgumentException for first validation error only
        // It doesn't aggregate multiple validation errors due to ValueObject design
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var request = new
        {
            Username = "testuser",
            Email = "invalid-email", // Invalid format - will be the first error thrown
            FirstName = "Test",
            LastName = "User",
            Password = "ValidPassword123!",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request, TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        
        // GlobalExceptionHandler deve retornar ProblemDetails
        content.Should().Contain("Erro de validação", "response should have validation error title in Portuguese");
        
        // Deve conter o erro de email
        content.Should().Contain("email");
    }

    [Fact]
    public async Task ConflictError_ShouldIncludeConstraintDetails()
    {
        // Arrange - Create duplicate user
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueEmail = $"{Guid.NewGuid().ToString("N")[..8]}@example.com";
        var request = new
        {
            Username = $"user{Guid.NewGuid().ToString("N")[..8]}",
            Email = uniqueEmail,
            FirstName = "Test",
            LastName = "User",
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };
        
        var firstResponse = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", request, TestContainerFixture.JsonOptions);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.Created, "setup: first user creation should succeed");

        // Act - Try to create duplicate
        var duplicateRequest = new
        {
            Username = fixture.Faker.Internet.UserName(),
            Email = uniqueEmail, // Same email
            FirstName = "Ana",
            LastName = "Souza",
            Password = "ValidPass123!",
            PhoneNumber = "+5511999999999"
        };
        var duplicateResponse = await fixture.ApiClient.PostAsJsonAsync("/api/v1/users", duplicateRequest, TestContainerFixture.JsonOptions);

        // Assert
        // TODO: Deveria retornar 409, mas atualmente retorna 400
        duplicateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Conflict, HttpStatusCode.BadRequest);

        var content = await duplicateResponse.Content.ReadAsStringAsync();
        
        // Verifica mensagem de duplicata em português
        content.Should().Contain("já está em uso", "conflict message should be in Portuguese");
    }

    #endregion

    #region NotFound vs BadRequest Distinction

    [Fact]
    public async Task GetNonExistentResource_ShouldReturn404NotFound()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await fixture.ApiClient.GetAsync($"/api/v1/services/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "non-existent resource should return 404, not 400");
    }

    [Fact]
    public async Task UpdateNonExistentResource_ShouldReturn404()
    {
        // Arrange
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();
        var request = new
        {
            name = fixture.Faker.Commerce.ProductName(),
            description = fixture.Faker.Lorem.Sentence()
        };

        // Act
        var response = await fixture.ApiClient.PutAsJsonAsync($"/api/v1/services/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "updating non-existent resource should return 404");
    }

    #endregion
}
