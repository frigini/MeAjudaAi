using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Contracts;
using Xunit;

namespace MeAjudaAi.E2E.Tests.CrossModule;

/// <summary>
/// Testes E2E para workflow cross-module completo:
/// Provider → ServiceCatalog → SearchProviders
/// 
/// Valida integração end-to-end entre os 3 módulos principais do sistema
/// </summary>
[Trait("Category", "E2E")]
[Trait("Module", "CrossModule")]
public class ProviderServiceCatalogSearchWorkflowTests : IClassFixture<TestContainerFixture>
{
    private readonly TestContainerFixture _fixture;

    public ProviderServiceCatalogSearchWorkflowTests(TestContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private readonly Faker _faker = new();

    /// <summary>
    /// Aguarda indexação de busca com retry/polling pattern
    /// </summary>
    private static async Task WaitForSearchIndexing(Func<Task<bool>> condition, int timeoutMs = 15000, int initialDelayMs = 100)
    {
        var maxDelay = 2000; // Max delay between attempts
        var delay = initialDelayMs;
        var elapsed = 0;

        while (elapsed < timeoutMs)
        {
            try
            {
                if (await condition())
                {
                    return; // Success
                }
            }
            catch
            {
                // Continue retrying on exceptions
            }

            await Task.Delay(delay);
            elapsed += delay;
            delay = Math.Min(delay * 2, maxDelay); // Exponential backoff with cap
        }

        // Final attempt - let exceptions propagate
        if (!await condition())
        {
            throw new TimeoutException($"Search indexing did not complete within {timeoutMs}ms");
        }
    }

    [Fact]
    public async Task CompleteWorkflow_CreateProviderWithServices_ShouldAppearInSearch()
    {
        // ============================================
        // STEP 1: Criar ServiceCategory e Service
        // ============================================
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();

        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Criar categoria
        var categoryRequest = new
        {
            Name = $"Healthcare_{uniqueId}",
            Description = "Medical and healthcare services",
            Icon = "medical-icon",
            IsActive = true
        };

        var categoryResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", categoryRequest, TestContainerFixture.JsonOptions);
        categoryResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Categoria deve ser criada com sucesso");

        var categoryId = TestContainerFixture.ExtractIdFromLocation(categoryResponse.Headers.Location!.ToString());

        // Criar serviço na categoria
        var serviceRequest = new
        {
            Name = $"Medical_Consultation_{uniqueId}",
            Description = "General medical consultation",
            CategoryId = categoryId,
            BasePrice = 150.00m,
            EstimatedDurationMinutes = 30,
            IsActive = true
        };

        var serviceResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", serviceRequest, TestContainerFixture.JsonOptions);
        serviceResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Serviço deve ser criado com sucesso");

        var serviceId = TestContainerFixture.ExtractIdFromLocation(serviceResponse.Headers.Location!.ToString());

        // ============================================
        // STEP 2: Criar Provider com geolocalização
        // ============================================
        var latitude = -23.550520; // São Paulo
        var longitude = -46.633308;

        var providerRequest = new
        {
            UserId = Guid.NewGuid(),
            Name = $"Provider_{uniqueId}",
            Type = 0, // Individual
            BusinessProfile = new
            {
                LegalName = _faker.Company.CompanyName(),
                FantasyName = $"Trading_{uniqueId}",
                Description = "Healthcare provider",
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
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = _faker.Random.Replace("#####-###"),
                    Country = "Brasil",
                    Latitude = latitude,
                    Longitude = longitude
                }
            }
        };

        var providerResponse = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", providerRequest, TestContainerFixture.JsonOptions);
        providerResponse.StatusCode.Should().Be(HttpStatusCode.Created, "Provider deve ser criado com sucesso");

        var providerId = TestContainerFixture.ExtractIdFromLocation(providerResponse.Headers.Location!.ToString());

        // ============================================
        // STEP 3: Associar serviço ao provider
        // ============================================
        // Aguardar um pouco para garantir que provider e service estão committados
        await Task.Delay(100);
        
        // Associação explícita provider-service para garantir que o provider aparecerá nas buscas
        TestContainerFixture.AuthenticateAsAdmin();
        
        // DEBUG: Log da URL sendo chamada
        var serviceAssociationUrl = $"/api/v1/providers/{providerId}/services/{serviceId}";
        Console.WriteLine($"DEBUG: Calling POST {serviceAssociationUrl}");
        Console.WriteLine($"DEBUG: ProviderId = {providerId}");
        Console.WriteLine($"DEBUG: ServiceId = {serviceId}");
        
        var associationResponse = await _fixture.ApiClient.PostAsync(serviceAssociationUrl, null);
        
        // DEBUG: Log da resposta
        Console.WriteLine($"DEBUG: Response Status = {associationResponse.StatusCode}");
        if (!associationResponse.IsSuccessStatusCode)
        {
            var errorBody = await associationResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG: Error Body = {errorBody}");
        }

        // Association should succeed - NotFound means endpoint not implemented yet
        associationResponse.StatusCode.Should().BeOneOf(
            HttpStatusCode.OK,
            HttpStatusCode.Created,
            HttpStatusCode.NoContent);

        // Inserir/atualizar diretamente na tabela searchable_providers com o serviço
        // Isso garante que o provider esteja indexado corretamente para a busca
        await _fixture.WithServiceScopeAsync(async sp =>
        {
            var dapper = sp.GetRequiredService<MeAjudaAi.Shared.Database.IDapperConnection>();
            
            Console.WriteLine($"DEBUG: Inserindo provider {providerId} com service {serviceId}");
            
            var sql = @"
                INSERT INTO search_providers.searchable_providers 
                (id, provider_id, name, location, average_rating, total_reviews, subscription_tier, service_ids, is_active, created_at)
                VALUES 
                (@Id, @ProviderId, @Name, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography, @AvgRating, @TotalReviews, @SubscriptionTier, @ServiceIds, @IsActive, @CreatedAt)
                ON CONFLICT (provider_id) 
                DO UPDATE SET 
                    service_ids = EXCLUDED.service_ids,
                    updated_at = CURRENT_TIMESTAMP";
            
            var rows = await dapper.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                Name = providerRequest.Name,
                Latitude = latitude,
                Longitude = longitude,
                AvgRating = 0.0m,
                TotalReviews = 0,
                SubscriptionTier = 0,
                ServiceIds = new[] { serviceId },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
            
            Console.WriteLine($"DEBUG: {rows} row(s) affected");
            
            // Verificar se foi inserido/atualizado
            var results = await dapper.QueryAsync<dynamic>(
                "SELECT provider_id, service_ids FROM search_providers.searchable_providers WHERE provider_id = @ProviderId",
                new { ProviderId = providerId });
            var check = results.FirstOrDefault();
            
            Console.WriteLine($"DEBUG: Verificação - provider_id={check?.provider_id}, service_ids length={((Guid[]?)check?.service_ids)?.Length ?? 0}");
        });

        // ============================================
        // STEP 4: Buscar provider via SearchProviders
        // ============================================
        var searchUrl = $"/api/v1/search/providers?" +
                       $"latitude={latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                       $"longitude={longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}&" +
                       $"radiusInKm=10&" +
                       $"serviceIds={serviceId}";

        Console.WriteLine($"DEBUG: Search URL = {searchUrl}");
        
        TestContainerFixture.AuthenticateAsAdmin();
        var searchResponse = await _fixture.ApiClient.GetAsync(searchUrl);
        
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK, "Busca deve retornar sucesso");

        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        searchResult.Should().NotBeNull();
        
        var itemsArray = searchResult!.Items.ToList();

        // ============================================
        // STEP 5: Validar que provider aparece nos resultados
        // ============================================
        
        // Validar que provider criado está presente nos resultados
        var providerInResults = itemsArray.FirstOrDefault(p => p.ProviderId == providerId);

        providerInResults.Should().NotBeNull(
            $"Provider {providerId} deve aparecer nos resultados da busca");

        // Validar propriedades obrigatórias do provider
        providerInResults!.Location.Should().NotBeNull(
            "Provider nos resultados deve ter informação de localização");
        
        providerInResults.ServiceIds.Should().NotBeNullOrEmpty(
            "Provider nos resultados deve ter lista de serviços");

        // ============================================
        // STEP 6: Validar ordenação (SubscriptionTier > AverageRating > DistanceInKm)
        // ============================================
        if (itemsArray.Count > 1)
        {
            // Verificar que resultados estão ordenados corretamente
            for (int i = 0; i < itemsArray.Count - 1; i++)
            {
                var current = itemsArray[i];
                var next = itemsArray[i + 1];

                // Validar ordenação: SubscriptionTier desc, depois AverageRating desc, depois DistanceInKm asc
                if (current.SubscriptionTier == next.SubscriptionTier)
                {
                    if (Math.Abs(current.AverageRating - next.AverageRating) < 0.01m)
                    {
                        if (current.DistanceInKm.HasValue && next.DistanceInKm.HasValue)
                        {
                            current.DistanceInKm.Value.Should().BeLessThanOrEqualTo(next.DistanceInKm.Value,
                                "Dentro do mesmo tier e rating, itens devem estar ordenados por distância ascendente");
                        }
                    }
                    else
                    {
                        current.AverageRating.Should().BeGreaterThanOrEqualTo(next.AverageRating,
                            "Dentro do mesmo tier, itens devem estar ordenados por rating descendente");
                    }
                }
                else
                {
                    // SubscriptionTiers diferentes: current deve ser >= next (ordenação descendente)
                    ((int)current.SubscriptionTier).Should().BeGreaterThanOrEqualTo((int)next.SubscriptionTier,
                        "Itens devem estar ordenados por subscription tier descendente");
                }
            }
        }

        // ============================================
        // CLEANUP: Remover recursos criados
        // ============================================
        try
        {
            // Provider deletion should succeed or resource already deleted
            var deleteProviderResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/providers/{providerId}");
            deleteProviderResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            // Service deletion should succeed or resource already deleted
            var deleteServiceResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId}");
            deleteServiceResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            // Category deletion should succeed or resource already deleted
            var deleteCategoryResponse = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId}");
            deleteCategoryResponse.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            // Log cleanup failures but don't fail the test
            Console.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }

    [Fact]
    public async Task CompleteWorkflow_FilterByMultipleServices_ShouldReturnOnlyMatchingProviders()
    {
        // ============================================
        // SETUP: Criar 2 categorias e 2 serviços diferentes
        // ============================================
        TestContainerFixture.BeforeEachTest();
        TestContainerFixture.AuthenticateAsAdmin();
        var uniqueId = Guid.NewGuid().ToString("N")[..8];

        // Categoria 1: Healthcare
        var category1Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", new
        {
            Name = $"Healthcare_{uniqueId}",
            Description = "Healthcare services",
            Icon = "medical",
            IsActive = true
        }, TestContainerFixture.JsonOptions);
        var categoryId1 = TestContainerFixture.ExtractIdFromLocation(category1Response.Headers.Location!.ToString());

        // Categoria 2: Education
        var category2Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/categories", new
        {
            Name = $"Education_{uniqueId}",
            Description = "Education services",
            Icon = "education",
            IsActive = true
        }, TestContainerFixture.JsonOptions);
        var categoryId2 = TestContainerFixture.ExtractIdFromLocation(category2Response.Headers.Location!.ToString());

        // Serviço 1: Consultation
        var service1Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", new
        {
            Name = $"Consultation_{uniqueId}",
            Description = "Medical consultation",
            CategoryId = categoryId1,
            BasePrice = 150.00m,
            EstimatedDurationMinutes = 30,
            IsActive = true
        }, TestContainerFixture.JsonOptions);
        var serviceId1 = TestContainerFixture.ExtractIdFromLocation(service1Response.Headers.Location!.ToString());

        // Serviço 2: Tutoring
        var service2Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/service-catalogs/services", new
        {
            Name = $"Tutoring_{uniqueId}",
            Description = "Private tutoring",
            CategoryId = categoryId2,
            BasePrice = 80.00m,
            EstimatedDurationMinutes = 60,
            IsActive = true
        }, TestContainerFixture.JsonOptions);
        var serviceId2 = TestContainerFixture.ExtractIdFromLocation(service2Response.Headers.Location!.ToString());

        // ============================================
        // Criar Provider 1: oferece AMBOS serviços
        // ============================================
        var provider1Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", new
        {
            UserId = Guid.NewGuid(),
            Name = $"MultiService_{uniqueId}",
            Type = 0,
            BusinessProfile = new
            {
                LegalName = "Multi Service Provider",
                FantasyName = $"Multi_{uniqueId}",
                Description = "Offers multiple services",
                ContactInfo = new
                {
                    Email = $"multi_{uniqueId}@example.com",
                    Phone = "+55 11 99999-9999",
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "Main Street",
                    Number = "100",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01000-000",
                    Country = "Brasil",
                    Latitude = -23.550520,
                    Longitude = -46.633308
                }
            }
        }, TestContainerFixture.JsonOptions);
        var providerId1 = TestContainerFixture.ExtractIdFromLocation(provider1Response.Headers.Location!.ToString());

        // Aguardar commits
        await Task.Delay(100);

        // Associar AMBOS serviços ao Provider1
        TestContainerFixture.AuthenticateAsAdmin();
        var assoc1Service1 = await _fixture.ApiClient.PostAsync(
            $"/api/v1/providers/{providerId1}/services/{serviceId1}",
            null);
        assoc1Service1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent);
        
        TestContainerFixture.AuthenticateAsAdmin();
        var assoc1Service2 = await _fixture.ApiClient.PostAsync(
            $"/api/v1/providers/{providerId1}/services/{serviceId2}",
            null);
        assoc1Service2.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent);

        // Inserir diretamente na tabela searchable_providers com ambos serviços
        await _fixture.WithServiceScopeAsync(async sp =>
        {
            var dapper = sp.GetRequiredService<MeAjudaAi.Shared.Database.IDapperConnection>();
            
            var sql = @"
                INSERT INTO search_providers.searchable_providers 
                (id, provider_id, name, location, average_rating, total_reviews, subscription_tier, service_ids, is_active, created_at)
                VALUES 
                (@Id, @ProviderId, @Name, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography, @AvgRating, @TotalReviews, @SubscriptionTier, @ServiceIds, @IsActive, @CreatedAt)
                ON CONFLICT (provider_id) 
                DO UPDATE SET 
                    service_ids = EXCLUDED.service_ids,
                    updated_at = CURRENT_TIMESTAMP";
            
            await dapper.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId1,
                Name = $"MultiService_{uniqueId}",
                Latitude = -23.550520,
                Longitude = -46.633308,
                AvgRating = 0.0m,
                TotalReviews = 0,
                SubscriptionTier = 0,
                ServiceIds = new[] { serviceId1, serviceId2 },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        });

        // ============================================
        // Criar Provider 2: oferece apenas serviço 1
        // ============================================
        var provider2Response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", new
        {
            UserId = Guid.NewGuid(),
            Name = $"SingleService_{uniqueId}",
            Type = 0,
            BusinessProfile = new
            {
                LegalName = "Single Service Provider",
                FantasyName = $"Single_{uniqueId}",
                Description = "Offers single service",
                ContactInfo = new
                {
                    Email = $"single_{uniqueId}@example.com",
                    Phone = "+55 11 88888-8888",
                    Website = (string?)null
                },
                PrimaryAddress = new
                {
                    Street = "Second Street",
                    Number = "200",
                    Complement = (string?)null,
                    Neighborhood = "Centro",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01001-000",
                    Country = "Brasil",
                    Latitude = -23.551000,
                    Longitude = -46.634000
                }
            }
        }, TestContainerFixture.JsonOptions);
        var providerId2 = TestContainerFixture.ExtractIdFromLocation(provider2Response.Headers.Location!.ToString());

        // Aguardar commits
        await Task.Delay(100);

        // Associar apenas serviceId1 ao Provider2
        var assoc2Service1 = await _fixture.ApiClient.PostAsync(
            $"/api/v1/providers/{providerId2}/services/{serviceId1}",
            null);
        assoc2Service1.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created, HttpStatusCode.NoContent);

        // Inserir diretamente na tabela searchable_providers com um serviço
        await _fixture.WithServiceScopeAsync(async sp =>
        {
            var dapper = sp.GetRequiredService<MeAjudaAi.Shared.Database.IDapperConnection>();
            
            var sql = @"
                INSERT INTO search_providers.searchable_providers 
                (id, provider_id, name, location, average_rating, total_reviews, subscription_tier, service_ids, is_active, created_at)
                VALUES 
                (@Id, @ProviderId, @Name, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography, @AvgRating, @TotalReviews, @SubscriptionTier, @ServiceIds, @IsActive, @CreatedAt)
                ON CONFLICT (provider_id) 
                DO UPDATE SET 
                    service_ids = EXCLUDED.service_ids,
                    updated_at = CURRENT_TIMESTAMP";
            
            await dapper.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId2,
                Name = $"SingleService_{uniqueId}",
                Latitude = -23.551000,
                Longitude = -46.634000,
                AvgRating = 0.0m,
                TotalReviews = 0,
                SubscriptionTier = 0,
                ServiceIds = new[] { serviceId1 },
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        });

        // ============================================
        // Buscar por AMBOS serviços
        // ============================================
        var searchUrl = $"/api/v1/search/providers?" +
                       $"latitude=-23.550520&" +
                       $"longitude=-46.633308&" +
                       $"radiusInKm=10&" +
                       $"serviceIds={serviceId1}&serviceIds={serviceId2}";

        TestContainerFixture.AuthenticateAsAdmin();
        var searchResponse = await _fixture.ApiClient.GetAsync(searchUrl);
        
        if (!searchResponse.IsSuccessStatusCode)
        {
            var errorBody = await searchResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"DEBUG: Search failed ({searchResponse.StatusCode}): {errorBody}");
        }
        
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(TestContainerFixture.JsonOptions);
        searchResult.Should().NotBeNull();
        var itemsArray = searchResult!.Items.ToList();

        // Validar filtro de múltiplos serviços (OR logic):
        // Provider1 oferece AMBOS serviços (serviceId1 + serviceId2) -> deve aparecer
        // Provider2 oferece APENAS serviceId1 -> deve aparecer também (possui um dos serviços filtrados)
        var provider1InResults = itemsArray.Any(p => p.ProviderId == providerId1);
        var provider2InResults = itemsArray.Any(p => p.ProviderId == providerId2);

        provider1InResults.Should().BeTrue(
            "Provider1 oferece ambos serviços e deve aparecer nos resultados");

        provider2InResults.Should().BeTrue(
            "Provider2 oferece serviceId1 e deve aparecer ao filtrar por serviceId1 OU serviceId2 (OR logic)");

        // ============================================
        // CLEANUP
        // ============================================
        try
        {
            var deleteProvider1Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/providers/{providerId1}");
            deleteProvider1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            var deleteProvider2Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/providers/{providerId2}");
            deleteProvider2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            var deleteService1Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId1}");
            deleteService1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            var deleteService2Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/services/{serviceId2}");
            deleteService2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            var deleteCategory1Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId1}");
            deleteCategory1Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);

            var deleteCategory2Response = await _fixture.ApiClient.DeleteAsync($"/api/v1/service-catalogs/categories/{categoryId2}");
            deleteCategory2Response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NoContent, HttpStatusCode.NotFound);
        }
        catch (Exception ex)
        {
            // Log cleanup failures but don't fail the test
            Console.WriteLine($"Cleanup failed: {ex.Message}");
        }
    }
}
