using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Ratings;

[Trait("Category", "E2E")]
[Trait("Module", "Ratings")]
public class RatingsEndToEndTests : BaseTestContainerTest
{
    protected override bool EnableEventsAndMessageBus => true;

    private readonly ITestOutputHelper _output;

    public RatingsEndToEndTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldUpdateProviderRatingInSearch()
    {
        // Arrange
        // Note: BaseTestContainerTest handles initialization and authentication setup
        
        // 1. Criar um prestador para ser avaliado
        _output.WriteLine("Step 1: Creating test provider...");
        AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        _output.WriteLine($"Provider created: {providerId}");
        
        // 2. Autenticar como cliente (diferente do prestador)
        _output.WriteLine("Step 2: Creating test user (customer)...");
        AuthenticateAsAdmin();
        var customerId = await CreateTestUserAsync();
        AuthenticateAsUser(customerId.ToString());
        _output.WriteLine($"Customer created: {customerId}");

        var reviewRequest = new
        {
            providerId = providerId,
            rating = 5,
            comment = "Excellent service!"
        };

        // Act - Criar a avaliação
        _output.WriteLine("Step 3: Posting review...");
        AuthenticateAsUser(customerId.ToString());
        var response = await ApiClient.PostAsJsonAsync("/api/v1/ratings", reviewRequest);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var body = await response.Content.ReadAsStringAsync();
            _output.WriteLine($"DEBUG RESPONSE BODY: {body}");
        }
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var reviewId = await response.Content.ReadFromJsonAsync<Guid>();
        reviewId.Should().NotBeEmpty();

        // 3. Verificar se a média foi atualizada no módulo de busca
        // O SynchronousInMemoryMessageBus garante que o processamento ocorreu antes do retorno da API
        // Usamos term=providerName para garantir que buscamos o prestador correto
        var searchResponse = await ApiClient.GetAsync($"/api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=10");
        searchResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(MeAjudaAi.Shared.Serialization.SerializationDefaults.Default);
        var providerInSearch = searchResult?.Items.FirstOrDefault(p => p.ProviderId == providerId);
        
        providerInSearch.Should().NotBeNull();
        // Nota 5 deve ser a média (já que é a única avaliação)
        providerInSearch!.AverageRating.Should().Be(5.0m);
        providerInSearch.TotalReviews.Should().Be(1);
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        AuthenticateAsAdmin();

        var userId = await CreateTestUserAsync();
        var name = $"Provider_{Guid.NewGuid():N}";
        var request = new
        {
            UserId = userId.ToString(),
            Name = name,
            Type = EProviderType.Individual,
            BusinessProfile = new
            {
                LegalName = name,
                FantasyName = name,
                Description = $"Test provider {name}",
                ContactInfo = new
                {
                    Email = $"{name}@example.com",
                    PhoneNumber = "+5511999999999"
                },
                PrimaryAddress = new
                {
                    Street = "Avenida Paulista",
                    Number = "1578",
                    Neighborhood = "Bela Vista",
                    City = "São Paulo",
                    State = "SP",
                    ZipCode = "01310-200",
                    Country = "Brasil"
                }
            }
        };

        var response = await ApiClient.PostAsJsonAsync("/api/v1/providers", request);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var providerId = ExtractIdFromLocation(location!);

        // Não tentaremos verificar/ativar via REST API porque as regras de estado (PendingBasicInfo -> etc)
        // requerem múltiplos endpoints que não são o foco deste teste.
        // Faremos a inserção direta do provider no SearchProviders (como em SearchProvidersEndToEndTests)
        // com rating = 0 para testar exclusivamente a integração de Reviews -> Search.
        await InsertSearchableProviderAsync(providerId, name, -23.5505, -46.6333);

        return providerId;
    }

    private async Task InsertSearchableProviderAsync(Guid providerId, string name, double latitude, double longitude)
    {
        await WithServiceScopeAsync(async sp =>
        {
            var dapper = sp.GetRequiredService<MeAjudaAi.Shared.Database.IDapperConnection>();
            
            var sql = @"
                INSERT INTO search_providers.searchable_providers 
                (id, provider_id, slug, name, description, city, state, location, average_rating, total_reviews, subscription_tier, service_ids, is_active, created_at, updated_at)
                VALUES 
                (@Id, @ProviderId, @Slug, @Name, @Description, @City, @State, ST_SetSRID(ST_MakePoint(@Longitude, @Latitude), 4326)::geography, @AvgRating, @TotalReviews, @SubscriptionTier, @ServiceIds, @IsActive, @CreatedAt, @UpdatedAt)
                ON CONFLICT (provider_id) 
                DO UPDATE SET 
                    average_rating = EXCLUDED.average_rating,
                    total_reviews = EXCLUDED.total_reviews,
                    updated_at = CURRENT_TIMESTAMP";
            
            await dapper.ExecuteAsync(sql, new
            {
                Id = Guid.NewGuid(),
                ProviderId = providerId,
                Slug = name.ToLowerInvariant().Replace(" ", "-").Replace("_", "-"),
                Name = name,
                Description = $"Test Provider {name}",
                City = "São Paulo",
                State = "SP",
                Latitude = latitude,
                Longitude = longitude,
                AvgRating = 0.0m, // Inicializa com 0
                TotalReviews = 0,
                SubscriptionTier = 1, // Standard
                ServiceIds = Array.Empty<Guid>(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        });
    }
}
