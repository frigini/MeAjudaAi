using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Providers;

/// <summary>
/// Testes E2E para os novos endpoints do módulo Providers:
/// POST /become, GET/PUT/DELETE /me, GET /me/status, POST /me/documents,
/// POST/DELETE /{providerId}/services/{serviceId}
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "Providers")]
public class ProvidersMeEndToEndTests(TestContainerFixture fixture) : BaseE2ETest<TestContainerFixture>(fixture)
{

    #region POST /become - Full Flow

    [Fact]
    public async Task BecomeProvider_CompleteFlow_ShouldCreateProvider()
    {
        // Arrange - Create a user first
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();

        // Switch auth to the new user
        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "newprovider");

        var becomeRequest = new
        {
            Name = "E2E Become Provider",
            Type = EProviderType.Individual,
            DocumentNumber = "12345678901",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/providers/become", becomeRequest, TestContainerFixture.JsonOptions);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Become provider failed: {response.StatusCode}. Body: {errorContent}");
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        var locationHeader = response.Headers.Location?.ToString();
        locationHeader.Should().NotBeNull();

        // The /become endpoint returns Location: /me (route name), so extract ID from response body
        if (!string.IsNullOrEmpty(locationHeader) && locationHeader.Contains("/me"))
        {
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(body, TestContainerFixture.JsonOptions);
            var data = json.TryGetProperty("data", out var d) ? d : json;
            _ = Guid.Parse(data.GetProperty("id").GetString()!);
        }
        else
        {
            _ =! string.IsNullOrEmpty(locationHeader)
                ? TestContainerFixture.ExtractIdFromLocation(locationHeader)
                : throw new InvalidOperationException("Could not extract provider ID from response");
        }

        // Verify provider exists via GET /me
        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "newprovider");
        var getMeResponse = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        getMeResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var meContent = await getMeResponse.Content.ReadAsStringAsync();
        meContent.Should().Contain("E2E Become Provider");
    }

    #endregion

    #region GET /me/status - Full Flow

    [Fact]
    public async Task GetMyProviderStatus_CompleteFlow_ShouldReturnStatus()
    {
        // Arrange - Create provider
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();

        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "statustest");
        var providerId = await CreateProviderViaBecomeAsync(userId);

        // Act - Get status
        var response = await Fixture.ApiClient.GetAsync("/api/v1/providers/me/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

        // Unwrap response envelope
        var data = json.TryGetProperty("value", out var value) ? value :
                   json.TryGetProperty("data", out var data2) ? data2 : json;

        data.TryGetProperty("status", out _).Should().BeTrue("status response should have status field");
        data.TryGetProperty("verificationStatus", out _).Should().BeTrue("status response should have verificationStatus field");
    }

    #endregion

    #region PUT /me - Full Flow

    [Fact]
    public async Task UpdateMyProviderProfile_CompleteFlow_ShouldPersistChanges()
    {
        // Arrange - Create provider
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();

        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "updatetest");
        await CreateProviderViaBecomeAsync(userId);

        var updateRequest = new
        {
            Name = "Updated E2E Provider",
            BusinessProfile = new
            {
                LegalName = "Updated Legal Name E2E",
                FantasyName = "Updated Fantasy E2E",
                Description = "Updated description for E2E test",
                ContactInfo = new
                {
                    Email = "updated-e2e@example.com",
                    PhoneNumber = "+5511888888888",
                    Website = "https://updated-e2e.example.com"
                },
                PrimaryAddress = new
                {
                    Street = "Updated E2E Street",
                    Number = "789",
                    Neighborhood = "Updated E2E Neighborhood",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        // Act
        var updateResponse = await Fixture.ApiClient.PutAsJsonAsync("/api/v1/providers/me", updateRequest, TestContainerFixture.JsonOptions);

        // Assert
        updateResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent);

        // Verify via GET
        var getResponse = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await getResponse.Content.ReadAsStringAsync();
        content.Should().Contain("Updated E2E Provider");
    }

    #endregion

    #region DELETE /me - Full Flow

    [Fact]
    public async Task DeleteMyProviderProfile_CompleteFlow_ShouldRemoveProvider()
    {
        // Arrange - Create provider
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();

        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "deletetest");
        await CreateProviderViaBecomeAsync(userId);

        // Verify provider exists
        var getBefore = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        getBefore.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Delete
        var deleteResponse = await Fixture.ApiClient.DeleteAsync("/api/v1/providers/me");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify provider is gone
        var getAfter = await Fixture.ApiClient.GetAsync("/api/v1/providers/me");
        getAfter.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /me/documents - Full Flow

    [Fact]
    public async Task UploadMyDocument_CompleteFlow_ShouldAddDocument()
    {
        // Arrange - Create provider
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();

        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "doctest");
        await CreateProviderViaBecomeAsync(userId);

        var documentRequest = new
        {
            Number = "123456789",
            DocumentType = EDocumentType.RG,
            FileName = "e2e_test_document.pdf",
            FileUrl = "https://blob.example.com/e2e_test_document.pdf"
        };

        // Act
        var response = await Fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/providers/me/documents",
            documentRequest,
            TestContainerFixture.JsonOptions);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Accepted);
    }

    #endregion

    #region POST/DELETE /{providerId}/services/{serviceId} - Full Flow

    [Fact]
    public async Task AddAndRemoveService_CompleteFlow_ShouldWork()
    {
        // Arrange - Create provider
        TestContainerFixture.AuthenticateAsAdmin();
        var userId = await Fixture.CreateTestUserAsync();
        var providerId = await CreateProviderViaBecomeAsync(userId);

        // Get a valid service ID from the service catalog
        var serviceId = await GetOrCreateTestServiceAsync();

        // Use admin auth: SelfOrAdminHandler compares sub claim with providerId route value,
        // but they are different GUIDs (user ID vs provider entity ID)
        TestContainerFixture.AuthenticateAsAdmin();

        // Act - Add service
        var addResponse = await Fixture.ApiClient.PostAsJsonAsync(
            $"/api/v1/providers/{providerId}/services/{serviceId}",
            (object?)null,
            TestContainerFixture.JsonOptions);

        // Assert - Add service
        addResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.OK,
            HttpStatusCode.Created);

        // Act - Remove service
        var removeResponse = await Fixture.ApiClient.DeleteAsync(
            $"/api/v1/providers/{providerId}/services/{serviceId}");

        // Assert - Remove service
        removeResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.OK);
    }

    #endregion

    #region Helper Methods

    private async Task<Guid> CreateProviderViaBecomeAsync(Guid userId)
    {
        TestContainerFixture.AuthenticateAsUser(userId.ToString(), "providerbecome");

        var becomeRequest = new
        {
            Name = $"E2E Provider {Guid.NewGuid().ToString("N")[..8]}",
            Type = EProviderType.Individual,
            DocumentNumber = "12345678901",
            PhoneNumber = "+5511999999999"
        };

        var response = await Fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/providers/become",
            becomeRequest,
            TestContainerFixture.JsonOptions);

        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Failed to create provider via become. Status: {response.StatusCode}, Body: {body}");
        }

        var location = response.Headers.Location?.ToString();
        if (!string.IsNullOrEmpty(location) && location.Contains("/me"))
        {
            var body = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(body, TestContainerFixture.JsonOptions);
            var data = json.GetProperty("data");
            return Guid.Parse(data.GetProperty("id").GetString()!);
        }
        if (!string.IsNullOrEmpty(location))
        {
            return TestContainerFixture.ExtractIdFromLocation(location);
        }

        throw new InvalidOperationException("Could not extract provider ID from response");
    }

    private async Task<Guid> GetOrCreateTestServiceAsync()
    {
        // Try to get an existing service from the catalog
        TestContainerFixture.AuthenticateAsAdmin();
        var response = await Fixture.ApiClient.GetAsync("/api/v1/service-catalogs/services");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var json = JsonSerializer.Deserialize<JsonElement>(content, TestContainerFixture.JsonOptions);

            // Unwrap response envelope
            var data = json.TryGetProperty("value", out var value) ? value :
                       json.TryGetProperty("data", out var data2) ? data2 : json;

            if (data.ValueKind == JsonValueKind.Array && data.GetArrayLength() > 0)
            {
                var firstService = data[0];
                if (firstService.TryGetProperty("id", out var idProp))
                {
                    return Guid.Parse(idProp.GetString()!);
                }
            }
        }

        // Fallback: create a category and service via API
        return await CreateTestServiceViaApiAsync();
    }

    private async Task<Guid> CreateTestServiceViaApiAsync()
    {
        TestContainerFixture.AuthenticateAsAdmin();

        // Create category
        var categoryRequest = new
        {
            Name = $"E2E Test Category {Guid.NewGuid().ToString("N")[..8]}",
            Description = "Test category for E2E provider service tests"
        };

        var categoryResponse = await Fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/categories",
            categoryRequest,
            TestContainerFixture.JsonOptions);

        if (!categoryResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create test category. Status: {categoryResponse.StatusCode}");
        }

        var categoryContent = await categoryResponse.Content.ReadAsStringAsync();
        var categoryJson = JsonSerializer.Deserialize<JsonElement>(categoryContent, TestContainerFixture.JsonOptions);
        var categoryData = categoryJson.TryGetProperty("value", out var catVal) ? catVal :
                           categoryJson.TryGetProperty("data", out var catData) ? catData : categoryJson;

        var categoryId = categoryData.TryGetProperty("id", out var catIdProp)
            ? Guid.Parse(catIdProp.GetString()!)
            : Guid.Parse(categoryResponse.Headers.Location?.ToString()?.Split('/')[^1] ?? Guid.Empty.ToString());

        // Create service
        var serviceRequest = new
        {
            CategoryId = categoryId.ToString(),
            Name = $"E2E Test Service {Guid.NewGuid().ToString("N")[..8]}",
            Description = "Test service for E2E provider service tests"
        };

        var serviceResponse = await Fixture.ApiClient.PostAsJsonAsync(
            "/api/v1/service-catalogs/services",
            serviceRequest,
            TestContainerFixture.JsonOptions);

        if (!serviceResponse.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failed to create test service. Status: {serviceResponse.StatusCode}");
        }

        var serviceContent = await serviceResponse.Content.ReadAsStringAsync();
        var serviceJson = JsonSerializer.Deserialize<JsonElement>(serviceContent, TestContainerFixture.JsonOptions);
        var serviceData = serviceJson.TryGetProperty("value", out var svcVal) ? svcVal :
                          serviceJson.TryGetProperty("data", out var svcData) ? svcData : serviceJson;

        return serviceData.TryGetProperty("id", out var svcIdProp)
            ? Guid.Parse(svcIdProp.GetString()!)
            : Guid.Parse(serviceResponse.Headers.Location?.ToString()?.Split('/')[^1] ?? Guid.Empty.ToString());
    }

    #endregion
}
