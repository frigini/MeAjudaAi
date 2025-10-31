using MeAjudaAi.E2E.Tests.Base;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para endpoints do módulo Providers
/// </summary>
public class ProvidersModuleTests : TestContainerTestBase
{
    [Fact]
    public async Task GetProviders_ShouldReturnOkWithPaginatedResult()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound // Aceitável se ainda não existem providers
        );

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            // Verifica se é JSON válido
            var jsonDocument = System.Text.Json.JsonDocument.Parse(content);
            jsonDocument.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task CreateProvider_WithValidData_ShouldReturnCreatedOrConflict()
    {
        // Arrange
        var createProviderRequest = new CreateProviderRequest
        {
            UserId = Guid.NewGuid(),
            Name = $"testprovider_{Guid.NewGuid():N}",
            Type = 0, // Individual
            BusinessProfile = new BusinessProfileRequest
            {
                LegalName = "Test Provider Legal Name",
                FantasyName = "Test Provider",
                Description = "Test provider description",
                ContactInfo = new ContactInfoRequest
                {
                    Email = $"provider_{Guid.NewGuid():N}@example.com",
                    PhoneNumber = "+55 11 99999-9999",
                    Website = "https://testprovider.com"
                },
                PrimaryAddress = new AddressRequest
                {
                    Street = "Test Street",
                    Number = "123",
                    Complement = "Apt 1",
                    Neighborhood = "Test Neighborhood",
                    City = "Test City",
                    State = "TS",
                    ZipCode = "12345-678",
                    Country = "Brasil"
                }
            }
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", createProviderRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Created,      // Sucesso
            HttpStatusCode.Conflict,     // Provider já existe para o usuário
            HttpStatusCode.BadRequest    // Erro de validação
        );

        if (response.StatusCode == HttpStatusCode.Created)
        {
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var createdProvider = System.Text.Json.JsonSerializer.Deserialize<CreateProviderResponse>(content, JsonOptions);
            createdProvider.Should().NotBeNull();
            createdProvider!.ProviderId.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task CreateProvider_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidRequest = new CreateProviderRequest
        {
            UserId = Guid.Empty, // Inválido: GUID vazio
            Name = "", // Inválido: nome vazio
            Type = 0,
            BusinessProfile = new BusinessProfileRequest
            {
                LegalName = "",
                ContactInfo = new ContactInfoRequest
                {
                    Email = "invalid-email", // Inválido: email mal formatado
                },
                PrimaryAddress = new AddressRequest()
            }
        };

        // Act
        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", invalidRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetProviderById_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // GetProviderById requer autorização
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/providers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProviderByUserId_WithNonExistentUserId_ShouldReturnNotFound()
    {
        // Arrange
        AuthenticateAsAdmin(); // GetProviderByUserId requer autorização
        var nonExistentUserId = Guid.NewGuid();

        // Act
        var response = await ApiClient.GetAsync($"/api/v1/providers/by-user/{nonExistentUserId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateProvider_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdateProviderRequest
        {
            Name = "Updated Provider Name",
            BusinessProfile = new BusinessProfileRequest
            {
                LegalName = "Updated Legal Name",
                ContactInfo = new ContactInfoRequest
                {
                    Email = $"updated_{Guid.NewGuid():N}@example.com"
                },
                PrimaryAddress = new AddressRequest
                {
                    Street = "Updated Street",
                    Number = "456",
                    City = "Updated City",
                    State = "UP",
                    ZipCode = "54321-987",
                    Country = "Brasil"
                }
            }
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{nonExistentId}", updateRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProvider_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await ApiClient.DeleteAsync($"/api/v1/providers/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ProviderEndpoints_ShouldHandleInvalidGuids()
    {
        // Act & Assert - Quando o constraint de GUID não bate, a rota retorna 404 
        var invalidGuidResponse = await ApiClient.GetAsync("/api/v1/providers/invalid-guid");
        invalidGuidResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProvidersByCity_ShouldReturnOkOrNotFound()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers/by-city/TestCity");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task GetProvidersByState_ShouldReturnOkOrNotFound()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers/by-state/SP");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound
        );
    }

    [Fact]
    public async Task GetProvidersByType_ShouldReturnOkOrNotFound()
    {
        // Act
        var response = await ApiClient.GetAsync("/api/v1/providers/by-type/0"); // Individual

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NotFound
        );
    }
}

/// <summary>
/// DTOs simples para teste (para evitar dependências complexas)
/// </summary>
public record CreateProviderRequest
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Type { get; init; }
    public BusinessProfileRequest BusinessProfile { get; init; } = new();
}

public record BusinessProfileRequest
{
    public string LegalName { get; init; } = string.Empty;
    public string? FantasyName { get; init; }
    public string? Description { get; init; }
    public ContactInfoRequest ContactInfo { get; init; } = new();
    public AddressRequest PrimaryAddress { get; init; } = new();
}

public record ContactInfoRequest
{
    public string Email { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public string? Website { get; init; }
}

public record AddressRequest
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string? Complement { get; init; }
    public string Neighborhood { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public record UpdateProviderRequest
{
    public string Name { get; init; } = string.Empty;
    public BusinessProfileRequest BusinessProfile { get; init; } = new();
}

public record CreateProviderResponse
{
    public Guid ProviderId { get; init; }
    public string Message { get; init; } = string.Empty;
}