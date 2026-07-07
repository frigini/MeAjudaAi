using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using System.Text.Json;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para os endpoints /me/* e demais novos endpoints do módulo Providers.
/// Cobre: POST /become, GET/PUT/DELETE /me, GET /me/status, POST /me/documents,
///        POST /{id}/require-correction, POST/DELETE /{providerId}/services/{serviceId}
/// </summary>
public class ProvidersMeEndpointsTests(ITestOutputHelper testOutput) : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers | TestModule.ServiceCatalogs | TestModule.Users;

    private async Task SeedTestServiceAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
        var categoryId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");
        var serviceId = TestServiceId;

        if (!await context.ServiceCategories.AnyAsync(c => c.Id == new ServiceCategoryId(categoryId)))
        {
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO service_catalogs.service_categories (id, name, description, is_active, display_order, created_at) VALUES ({0}, 'Test Category', NULL, true, 0, NOW()) ON CONFLICT DO NOTHING",
                categoryId);
        }

        if (!await context.Services.AnyAsync(s => s.Id == new ServiceId(serviceId)))
        {
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO service_catalogs.services (id, category_id, name, description, is_active, display_order, created_at) VALUES ({0}, {1}, 'Test Service', NULL, true, 0, NOW()) ON CONFLICT DO NOTHING",
                serviceId, categoryId);
        }
    }

    #region POST /become

    [Fact]
    public async Task BecomeProvider_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "newprovider", "newprovider@test.com");

        var becomeRequest = new
        {
            Name = "New Provider Corp",
            Type = EProviderType.Individual,
            DocumentNumber = "12345678901",
            PhoneNumber = "+5511999999999"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/become", becomeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            "POST /become with valid data should return 201 Created");
        testOutput.WriteLine($"POST /become response: {response.StatusCode}");

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        var data = GetResponseData(json);

        data.TryGetProperty("id", out _).Should().BeTrue("response should contain provider id");
        data.TryGetProperty("name", out var nameProp).Should().BeTrue();
        nameProp.GetString().Should().Be("New Provider Corp");

        // Cleanup
        if (data.TryGetProperty("id", out var idProp))
        {
            var providerId = idProp.GetString();
            await Client.DeleteAsync($"/api/v1/providers/{providerId}");
        }
    }

    [Fact]
    public async Task BecomeProvider_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();

        var becomeRequest = new
        {
            Name = "Should Fail",
            Type = EProviderType.Individual,
            DocumentNumber = "12345678901"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/become", becomeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task BecomeProvider_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "invalidprovider", "invalid@test.com");

        var becomeRequest = new
        {
            Name = "", // Empty name - invalid
            Type = EProviderType.Individual,
            DocumentNumber = "" // Empty document - invalid
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/become", becomeRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task BecomeProvider_WhenAlreadyProvider_ShouldReturnBadRequest()
    {
        // Arrange - Create a provider first via direct DB insertion
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Existing Provider")
            .AsIndividual()
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        // Try to become provider again
        AuthConfig.ConfigureUser(userId.ToString(), "existingprovider", "existing@test.com");

        var becomeRequest = new
        {
            Name = "Another Provider",
            Type = EProviderType.Company,
            DocumentNumber = "98765432100"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/become", becomeRequest);

        // Assert - should fail because user already has a provider
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict);
    }

    #endregion

    #region GET /me/status

    [Fact]
    public async Task GetMyProviderStatus_WithValidProvider_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Status Test Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.Active)
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureUser(userId.ToString(), "statususer", "status@test.com");

        // Act
        var response = await Client.GetAsync("/api/v1/providers/me/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonSerializer.Deserialize<JsonElement>(content);
        var data = GetResponseData(json);

        data.TryGetProperty("status", out _).Should().BeTrue("response should contain status");
        data.TryGetProperty("verificationStatus", out _).Should().BeTrue("response should contain verificationStatus");
    }

    [Fact]
    public async Task GetMyProviderStatus_WithoutProvider_ShouldReturnNotFound()
    {
        // Arrange - authenticated user without a provider
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "noprovider", "noprovider@test.com");

        // Act
        var response = await Client.GetAsync("/api/v1/providers/me/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMyProviderStatus_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();

        // Act
        var response = await Client.GetAsync("/api/v1/providers/me/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region PUT /me

    [Fact]
    public async Task UpdateMyProviderProfile_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Update Test Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.Active)
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureUser(userId.ToString(), "updateuser", "update@test.com");

        var updateRequest = new
        {
            Name = "Updated Provider Name",
            BusinessProfile = new
            {
                LegalName = "Updated Legal Name",
                FantasyName = "Updated Fantasy",
                Description = "Updated description",
                ContactInfo = new
                {
                    Email = "updated@example.com",
                    PhoneNumber = "+5511888888888"
                },
                PrimaryAddress = new
                {
                    Street = "Updated Street",
                    Number = "456",
                    Neighborhood = "Updated Neighborhood",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01234-567",
                    Country = "Brasil"
                }
            }
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/providers/me", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Updated Provider Name");
    }

    [Fact]
    public async Task UpdateMyProviderProfile_WithoutProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "noupdate", "noupdate@test.com");

        var updateRequest = new
        {
            Name = "Should Not Work"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/providers/me", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateMyProviderProfile_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Invalid Update Provider")
            .AsIndividual()
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureUser(userId.ToString(), "invalidupdate", "invalidupdate@test.com");

        var updateRequest = new
        {
            Name = "", // Invalid - empty
            BusinessProfile = new
            {
                LegalName = new string('a', 201), // Invalid - too long
                ContactInfo = new
                {
                    Email = "not-an-email"
                }
            }
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/v1/providers/me", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /me

    [Fact]
    public async Task DeleteMyProviderProfile_WithValidProvider_ShouldReturnNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Delete Test Provider")
            .AsIndividual()
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureUser(userId.ToString(), "deleteuser", "delete@test.com");

        // Act
        var response = await Client.DeleteAsync("/api/v1/providers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify provider is deleted
        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            var deletedProvider = await context.Providers.FirstOrDefaultAsync(p => p.UserId == userId);
            deletedProvider.Should().NotBeNull("provider entity should still exist after soft delete");
            deletedProvider!.IsDeleted.Should().BeTrue("provider should be soft-deleted after DELETE /me");
        }
    }

    [Fact]
    public async Task DeleteMyProviderProfile_WithoutProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "nodelete", "nodelete@test.com");

        // Act
        var response = await Client.DeleteAsync("/api/v1/providers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMyProviderProfile_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();

        // Act
        var response = await Client.DeleteAsync("/api/v1/providers/me");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /me/documents

    [Fact]
    public async Task UploadMyDocument_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Document Test Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.Active)
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureUser(userId.ToString(), "docuser", "doc@test.com");

        var documentRequest = new
        {
            Number = "123456789",
            DocumentType = EDocumentType.RG,
            FileName = "test_document.pdf",
            FileUrl = "https://blob.example.com/test_document.pdf"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/me/documents", documentRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.Accepted);
    }

    [Fact]
    public async Task UploadMyDocument_WithoutProvider_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        AuthConfig.ConfigureUser(userId.ToString(), "nodocprovider", "nodoc@test.com");

        var documentRequest = new
        {
            Number = "123456789",
            DocumentType = EDocumentType.RG
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/v1/providers/me/documents", documentRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /{id}/require-correction

    [Fact]
    public async Task RequireBasicInfoCorrection_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithUserId(Guid.NewGuid())
            .WithName("Correction Test Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.PendingDocumentVerification)
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureAdmin();

        var correctionRequest = new
        {
            Reason = "Please update your business profile information"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{providerId}/require-basic-info-correction", correctionRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RequireBasicInfoCorrection_WithoutPermission_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithUserId(Guid.NewGuid())
            .WithName("Forbidden Correction Provider")
            .AsIndividual()
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        // Configure as regular user without ProvidersApprove permission
        AuthConfig.ConfigureRegularUser();

        var correctionRequest = new
        {
            Reason = "Should not work"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{providerId}/require-basic-info-correction", correctionRequest);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.Forbidden,
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RequireBasicInfoCorrection_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentId = Guid.NewGuid();

        var correctionRequest = new
        {
            Reason = "Provider does not exist"
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{nonExistentId}/require-basic-info-correction", correctionRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /{providerId}/services/{serviceId}

    [Fact]
    public async Task AddServiceToProvider_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        await SeedTestServiceAsync();
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Service Test Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.Active)
            .Build();

        // Get a valid service ID from ServiceCatalogs
        var serviceId = TestServiceId;

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{provider.Id.Value}/services/{serviceId}", (object?)null);

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.OK,
            HttpStatusCode.Created);
    }

    [Fact]
    public async Task AddServiceToProvider_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentProviderId = Guid.NewGuid();
        var serviceId = TestServiceId;

        // Act
        var response = await Client.PostAsJsonAsync($"/api/v1/providers/{nonExistentProviderId}/services/{serviceId}", (object?)null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /{providerId}/services/{serviceId}

    [Fact]
    public async Task RemoveServiceFromProvider_WithValidData_ShouldReturnNoContent()
    {
        // Arrange
        await SeedTestServiceAsync();
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithUserId(userId)
            .WithName("Remove Service Provider")
            .AsIndividual()
            .WithStatus(EProviderStatus.Active)
            .Build();

        var serviceId = TestServiceId;

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();

            // Add service to provider first
            provider.AddService(serviceId, "Test Service");
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureAdmin();

        // Act
        var response = await Client.DeleteAsync($"/api/v1/providers/{provider.Id.Value}/services/{serviceId}");

        // Assert
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.NoContent,
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task RemoveServiceFromProvider_WithNonExistentProvider_ShouldReturnNotFound()
    {
        // Arrange
        AuthConfig.ConfigureAdmin();
        var nonExistentProviderId = Guid.NewGuid();
        var serviceId = TestServiceId;

        // Act
        var response = await Client.DeleteAsync($"/api/v1/providers/{nonExistentProviderId}/services/{serviceId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /{id}/verification-events (SSE)

    [Fact]
    public async Task GetVerificationEvents_WithValidProvider_ShouldReturnEventStream()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithUserId(Guid.NewGuid())
            .WithName("SSE Test Provider")
            .AsIndividual()
            .Build();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            context.Providers.Add(provider);
            await context.SaveChangesAsync();
        }

        AuthConfig.ConfigureAdmin();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/providers/{providerId}/verification-events");
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("text/event-stream");
    }

    [Fact]
    public async Task GetVerificationEvents_WithoutAuth_ShouldReturn401()
    {
        // Arrange
        AuthConfig.ClearConfiguration();
        var providerId = Guid.NewGuid();

        // Act
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/providers/{providerId}/verification-events");
        using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
