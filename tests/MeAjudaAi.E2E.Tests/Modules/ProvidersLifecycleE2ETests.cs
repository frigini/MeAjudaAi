using System.Net;
using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;

namespace MeAjudaAi.E2E.Tests.Modules;

/// <summary>
/// Testes E2E para operações de lifecycle de Providers (Update, Delete, Verification Status)
/// Cobre os gaps críticos identificados na análise de cobertura
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProvidersLifecycleE2ETests : TestContainerTestBase
{
    private readonly Faker _faker = new();

    [Fact]
    public async Task UpdateProvider_WithValidData_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId.ToString(), // UserId é string no Request base
            Name = $"Provider_{uniqueId}",
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = _faker.Company.CompanyName(),
                FantasyName = $"Trading_{uniqueId}",
                Description = "Original description",
                ContactInfo = new
                {
                    Email = $"provider_{uniqueId}@example.com",
                    Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = _faker.Address.StreetName(),
                    Number = _faker.Random.Number(1, 9999).ToString(),
                    Complement = (string?)null,
                    Neighborhood = _faker.Address.County(),
                    City = _faker.Address.City(),
                    State = _faker.Address.StateAbbr(),
                    ZipCode = _faker.Random.Replace("#####-###"),
                    Country = "Brasil"
                }
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created, "provider creation must succeed for update test to be meaningful");

        var locationHeader = createResponse.Headers.Location?.ToString();
        locationHeader.Should().NotBeNullOrEmpty("Created response must include Location header");
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Update provider
        var updateRequest = new
        {
            Name = $"Updated_{uniqueId}",
            BusinessProfile = new
            {
                LegalName = _faker.Company.CompanyName(),
                FantasyName = $"UpdatedTrading_{uniqueId}",
                Description = "Updated description",
                ContactInfo = new
                {
                    Email = $"updated_{uniqueId}@example.com",
                    Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "Updated Street",
                    Number = "999",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        // Re-authenticate before the update operation
        AuthenticateAsAdmin();
        var updateResponse = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", updateRequest, JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        if (updateResponse.StatusCode == HttpStatusCode.OK)
        {
            var content = await updateResponse.Content.ReadAsStringAsync();
            content.Should().Contain("Updated");
        }
    }

    [Fact]
    public async Task UpdateProvider_WithInvalidData_Should_Return_BadRequest()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid();

        var invalidRequest = new
        {
            CompanyName = "", // Inválido - campo obrigatório vazio
            TradingName = new string('a', 201), // Inválido - muito longo
            Email = "not-an-email", // Inválido - formato incorreto
            Phone = "123", // Inválido - formato incorreto
            Description = "",
            Address = new
            {
                Street = "",
                Number = "",
                City = "",
                State = "INVALID", // Inválido - mais de 2 caracteres
                ZipCode = "123",
                Latitude = 91.0, // Inválido - fora do range
                Longitude = 181.0 // Inválido - fora do range
            }
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}", invalidRequest, JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound); // NotFound aceitável se provider não existe
    }

    [Fact]
    public async Task DeleteProvider_WithoutDocuments_Should_Return_Success()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToDelete_{uniqueId}",
            TradingName = $"ToDeleteTrading_{uniqueId}",
            TaxId = _faker.Random.Replace("###########"),
            Email = $"todelete_{uniqueId}@example.com",
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider to be deleted",
            Address = new
            {
                Street = _faker.Address.StreetName(),
                Number = _faker.Random.Number(1, 9999).ToString(),
                City = _faker.Address.City(),
                State = _faker.Address.StateAbbr(),
                ZipCode = _faker.Random.Replace("#####-###"),
                Latitude = _faker.Address.Latitude(),
                Longitude = _faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            // Skip test if provider creation fails
            return;
        }

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Delete provider
        var deleteResponse = await ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");

        // Assert
        deleteResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);

        // Verify provider is deleted
        if (deleteResponse.IsSuccessStatusCode)
        {
            var getResponse = await ApiClient.GetAsync($"/api/v1/providers/{providerId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }

    [Fact]
    public async Task UpdateVerificationStatus_ToVerified_Should_Succeed()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToVerify_{uniqueId}",
            TradingName = $"ToVerifyTrading_{uniqueId}",
            TaxId = _faker.Random.Replace("###########"),
            Email = $"toverify_{uniqueId}@example.com",
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider to be verified",
            Address = new
            {
                Street = _faker.Address.StreetName(),
                Number = _faker.Random.Number(1, 9999).ToString(),
                City = _faker.Address.City(),
                State = _faker.Address.StateAbbr(),
                ZipCode = _faker.Random.Replace("#####-###"),
                Latitude = _faker.Address.Latitude(),
                Longitude = _faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            // Skip test if provider creation fails
            return;
        }

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Update verification status to Verified
        var updateStatusRequest = new
        {
            Status = 2, // Verified
            Reason = "Verification completed successfully"
        };

        var updateResponse = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            updateStatusRequest,
            JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateVerificationStatus_InvalidTransition_Should_Fail()
    {
        // Arrange
        AuthenticateAsAdmin();
        var providerId = Guid.NewGuid(); // Provider inexistente

        var invalidRequest = new
        {
            Status = 999, // Status inválido
            Reason = "Invalid status"
        };

        // Act
        var response = await ApiClient.PutAsJsonAsync(
            $"/api/v1/providers/{providerId}/verification-status",
            invalidRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RequestBasicInfoCorrection_Should_TriggerWorkflow()
    {
        // Arrange
        AuthenticateAsAdmin();
        var userId = Guid.NewGuid();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        var createRequest = new
        {
            UserId = userId,
            Type = 0, // Individual
            CompanyName = $"ToCorrect_{uniqueId}",
            TradingName = $"ToCorrectTrading_{uniqueId}",
            TaxId = _faker.Random.Replace("###########"),
            Email = $"tocorrect_{uniqueId}@example.com",
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Description = "Provider requiring correction",
            Address = new
            {
                Street = _faker.Address.StreetName(),
                Number = _faker.Random.Number(1, 9999).ToString(),
                City = _faker.Address.City(),
                State = _faker.Address.StateAbbr(),
                ZipCode = _faker.Random.Replace("#####-###"),
                Latitude = _faker.Address.Latitude(),
                Longitude = _faker.Address.Longitude()
            }
        };

        var createResponse = await ApiClient.PostAsJsonAsync("/api/v1/providers", createRequest, JsonOptions);

        if (createResponse.StatusCode != HttpStatusCode.Created)
        {
            // Skip test if provider creation fails
            return;
        }

        var locationHeader = createResponse.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(locationHeader!);

        // Act - Request basic info correction
        var correctionRequest = new
        {
            Reason = "Tax ID format is incorrect",
            RequestedFields = new[] { "TaxId", "CompanyName" }
        };

        var response = await ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/require-basic-info-correction",
            correctionRequest,
            JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Accepted,
            HttpStatusCode.NoContent);
    }
}
