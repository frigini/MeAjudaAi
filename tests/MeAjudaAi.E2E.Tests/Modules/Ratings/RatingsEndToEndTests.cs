using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Ratings;

[Trait("Category", "E2E")]
[Trait("Module", "Ratings")]
public class RatingsEndToEndTests : IClassFixture<TestContainerFixture>, IAsyncLifetime
{
    private readonly TestContainerFixture _fixture;
    private readonly ITestOutputHelper _output;

    public RatingsEndToEndTests(TestContainerFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    public async ValueTask InitializeAsync()
    {
        await _fixture.CleanupDatabaseAsync();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task CreateReview_WithValidData_ShouldUpdateProviderRatingInSearch()
    {
        // Arrange
        // 1. Criar um prestador para ser avaliado
        _output.WriteLine("Step 1: Creating provider...");
        TestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        _output.WriteLine($"Provider created: {providerId}");
        
        // 2. Autenticar como cliente (diferente do prestador)
        _output.WriteLine("Step 2: Creating customer...");
        TestContainerFixture.AuthenticateAsAdmin();
        var customerId = await _fixture.CreateTestUserAsync();
        _output.WriteLine($"Customer created: {customerId}");
        TestContainerFixture.AuthenticateAsUser(customerId.ToString());

        var reviewRequest = new
        {
            providerId = providerId,
            rating = 5,
            comment = "" // Sem comentário para disparar auto-aprovação e atualizar busca
        };

        // Act - Criar a avaliação
        _output.WriteLine("Act: Creating review...");
        
        TestContainerFixture.AuthenticateAsUser(customerId.ToString());
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/ratings", reviewRequest);
        
        _output.WriteLine($"Review response: {response.StatusCode}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var reviewId = await response.Content.ReadFromJsonAsync<Guid>();
        reviewId.Should().NotBeEmpty();

        // 3. Verificar se a média foi atualizada no módulo de busca
        // O SynchronousInMemoryMessageBus garante que o processamento ocorreu antes do retorno da API
        var searchResponse = await _fixture.ApiClient.GetAsync($"/api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=10");
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<SearchableProviderDto>>(MeAjudaAi.Shared.Serialization.SerializationDefaults.Default);
        var providerInSearch = searchResult?.Items.FirstOrDefault(p => p.ProviderId == providerId);
        
        providerInSearch.Should().NotBeNull();
        // Nota 5 deve ser a média (já que é a única avaliação)
        providerInSearch!.AverageRating.Should().Be(5.0m);
        providerInSearch.TotalReviews.Should().Be(1);
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        TestContainerFixture.AuthenticateAsAdmin();

        var userId = await _fixture.CreateTestUserAsync();
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

        _output.WriteLine("Posting to /api/v1/providers...");
        
        var response = await _fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request);
        
        _output.WriteLine($"Providers response: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var providerId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Inserção direta do provider no SearchProviders com rating = 0
        // para testar exclusivamente a integração de Reviews → Search.
        await InsertSearchableProviderAsync(providerId, name, -23.5505, -46.6333);

        return providerId;
    }

    private async Task InsertSearchableProviderAsync(Guid providerId, string name, double latitude, double longitude)
    {
        await _fixture.WithServiceScopeAsync(async sp =>
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
