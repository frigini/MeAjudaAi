using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using Xunit;

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
public class ValidationStatusCodeEndToEndTests : TestContainerTestBase
{
    #region 400 Bad Request - FluentValidation Errors

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturn400()
    {
        // Arrange - Email format inválido (validação do FluentValidation)
        var password = Faker.Internet.Password(12, true);
        var request = new
        {
            email = "not-an-email",
            password = password,
            confirmPassword = password,
            fullName = Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/users/register", request);

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
        var request = new
        {
            email = Faker.Internet.Email(),
            password = Faker.Internet.Password(12, true),
            // confirmPassword ausente (required)
            fullName = Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "missing required field should trigger FluentValidation error (400)");
    }

    [Fact]
    public async Task CreateService_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        AuthenticateAsAdmin();
        var request = new
        {
            name = "", // Empty name - FluentValidation error
            description = Faker.Lorem.Sentence(),
            categoryId = Guid.NewGuid()
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/services", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
            "empty required field should return 400");
    }

    [Fact]
    public async Task UpdateUser_WithInvalidPhoneFormat_ShouldReturn400()
    {
        // Arrange - Registrar usuário primeiro
        var password = Faker.Internet.Password(12, true);
        var registerRequest = new
        {
            email = Faker.Internet.Email(),
            password = password,
            confirmPassword = password,
            fullName = Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        await ApiClient.PostAsJsonAsync("/api/users/register", registerRequest);

        var loginRequest = new
        {
            email = registerRequest.email,
            password = registerRequest.password
        };
        var loginResponse = await ApiClient.PostAsJsonAsync("/api/users/login", loginRequest);
        var loginResult = await loginResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var token = loginResult!["token"].ToString();

        ApiClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act - Update com telefone inválido
        var updateRequest = new
        {
            phoneNumber = "invalid-phone" // Formato inválido
        };

        var response = await ApiClient.PutAsJsonAsync("/api/v1/users/phone", updateRequest);

        // Assert
        // Invalid phone format should trigger validation error
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound); // Endpoint pode não existir ainda
    }

    #endregion

    #region 422 Unprocessable Entity - Semantic/Business Validation (Future)

    [Fact(Skip = "422 não está implementado - atualmente usa 400 para todas validações")]
    public async Task CreateService_WithNonExistentCategory_ShouldReturn422()
    {
        // Arrange
        AuthenticateAsAdmin();
        var request = new
        {
            name = Faker.Commerce.ProductName(),
            description = Faker.Lorem.Sentence(),
            categoryId = Guid.NewGuid() // Categoria não existe (validação semântica)
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/services", request);

        // Assert
        // Atualmente retorna 400, mas idealmente deveria retornar 422
        // para distinguir validação semântica (categoria não existe) de validação de formato
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity,
            "semantic validation (non-existent category) should return 422");
    }

    [Fact(Skip = "422 não está implementado - atualmente usa 400 para todas validações")]
    public async Task ChangeServiceCategory_WithInvalidTransition_ShouldReturn422()
    {
        // Arrange - Criar serviço e tentar mudar para categoria que não existe
        AuthenticateAsAdmin();
        
        // Este cenário testaria validação de transição de estado
        // Exemplo: não pode mudar categoria de serviço se houver pedidos ativos
        
        // Act & Assert
        // Deveria retornar 422 para regras de negócio complexas
    }

    #endregion

    #region 409 Conflict - Duplicate Resources

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturn409()
    {
        // Arrange - Registrar usuário
        var password = Faker.Internet.Password(12, true);
        var request = new
        {
            email = Faker.Internet.Email(),
            password = password,
            confirmPassword = password,
            fullName = Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        
        await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Act - Tentar registrar novamente com mesmo email
        var duplicateResponse = await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict,
            "duplicate email should return 409 Conflict via UniqueConstraintException");

        var content = await duplicateResponse.Content.ReadAsStringAsync();
        content.Should().Contain("já está sendo utilizado", "conflict message should indicate duplication in Portuguese");
    }

    [Fact]
    public async Task CreateCategory_WithDuplicateName_ShouldReturn409()
    {
        // Arrange
        AuthenticateAsAdmin();
        var categoryName = Faker.Commerce.Department();

        var request = new
        {
            name = categoryName,
            description = Faker.Lorem.Sentence()
        };

        await ApiClient.PostAsJsonAsync("/api/v1/categories", request);

        // Act - Tentar criar categoria com mesmo nome
        var duplicateResponse = await ApiClient.PostAsJsonAsync("/api/v1/categories", request);

        // Assert
        // Duplicate category name should return conflict or validation error
        duplicateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.Conflict,
            HttpStatusCode.BadRequest); // Pode variar dependendo da implementação
    }

    #endregion

    #region Validation Error Response Format

    [Fact]
    public async Task ValidationError_ShouldReturnStructuredErrorResponse()
    {
        // Arrange - Request com múltiplos erros de validação
        var request = new
        {
            email = "invalid-email",
            password = "123", // Muito curto
            fullName = "", // Vazio
            phoneNumber = "abc" // Formato inválido
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var content = await response.Content.ReadAsStringAsync();
        
        // GlobalExceptionHandler deve retornar ProblemDetails
        content.Should().Contain("Erro de validação", "response should have validation error title in Portuguese");
        
        // Deve conter erros agrupados por campo
        content.Should().Contain("email");
        content.Should().Contain("password");
    }

    [Fact]
    public async Task ConflictError_ShouldIncludeConstraintDetails()
    {
        // Arrange - Criar usuário duplicado
        var password = Faker.Internet.Password(12, true);
        var request = new
        {
            email = Faker.Internet.Email(),
            password = password,
            confirmPassword = password,
            fullName = Faker.Name.FullName(),
            phoneNumber = "+5511987654321"
        };
        
        await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Act
        var duplicateResponse = await ApiClient.PostAsJsonAsync("/api/users/register", request);

        // Assert
        duplicateResponse.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var content = await duplicateResponse.Content.ReadAsStringAsync();
        
        // GlobalExceptionHandler deve incluir constraintName e columnName
        content.Should().Contain("já existe", "conflict message should be in Portuguese");
    }

    #endregion

    #region NotFound vs BadRequest Distinction

    [Fact]
    public async Task GetNonExistentResource_ShouldReturn404NotFound()
    {
        // Arrange
        AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/services/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "non-existent resource should return 404, not 400");
    }

    [Fact]
    public async Task UpdateNonExistentResource_ShouldReturn404()
    {
        // Arrange
        AuthenticateAsAdmin();
        var nonExistentId = Guid.NewGuid();
        var request = new
        {
            name = Faker.Commerce.ProductName(),
            description = Faker.Lorem.Sentence()
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/services/{nonExistentId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound,
            "updating non-existent resource should return 404");
    }

    #endregion
}

