using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Contracts.Modules.SearchProviders.DTOs;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using System.Net.Http.Json;

namespace MeAjudaAi.E2E.Tests.Modules.Ratings;

[Trait("Category", "E2E")]
[Trait("Module", "Ratings")]
public class RatingsEndToEndTests(EventsEnabledTestContainerFixture fixture) : BaseEventsE2ETest(fixture)
{

    [Fact]
    public async Task CreateReview_WithValidData_ShouldUpdateProviderRatingInSearch()
    {
        // Arrange
        // Note: BaseTestContainerTest handles initialization and authentication setup
        
        // 1. Criar um prestador para ser avaliado
        EventsEnabledTestContainerFixture.AuthenticateAsAdmin();
        var providerId = await CreateTestProviderAsync();
        
        // 2. Autenticar como cliente (diferente do prestador)
        var customerId = await Fixture.CreateTestUserAsync();
        
        // --- NOVO: Criar agendamento concluído ---
        // Para simplificar, faremos a inserção direta no banco de dados para evitar o fluxo complexo de criação/confirmação
        await Fixture.WithServiceScopeAsync(async sp =>
        {
            var db = sp.GetRequiredService<MeAjudaAi.Modules.Bookings.Infrastructure.Persistence.BookingsDbContext>();
            var booking = MeAjudaAi.Modules.Bookings.Domain.Entities.Booking.Create(
                providerId, 
                customerId, 
                Guid.NewGuid(), 
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), 
                MeAjudaAi.Modules.Bookings.Domain.ValueObjects.TimeSlot.Create(new TimeOnly(10, 0), new TimeOnly(11, 0)));
            
            // Força o status para Confirmed e depois Completed
            booking.Confirm();
            booking.Complete(); 
            
            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
        });
        // --- FIM NOVO ---

        TestContainerFixture.AuthenticateAsUser(customerId.ToString());

        var reviewRequest = new
        {
            providerId = providerId,
            rating = 5,
            comment = "" // Sem comentário para disparar auto-aprovação e atualizar busca
        };

        // Act - Criar a avaliação
        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/ratings", reviewRequest);


        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var reviewId = await response.Content.ReadFromJsonAsync<Guid>();
        reviewId.Should().NotBeEmpty();

        // 3. Verificar se a média foi atualizada no módulo de busca
        // O SynchronousInMemoryMessageBus garante que o processamento ocorreu antes do retorno da API
        // Usamos term=providerName para garantir que buscamos o prestador correto
        var searchResponse = await Fixture.ApiClient.GetAsync($"/api/v1/search/providers?latitude=-23.5505&longitude=-46.6333&radiusInKm=10");
        searchResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        
        var searchResult = await searchResponse.Content.ReadFromJsonAsync<PagedResult<ModuleSearchableProviderDto>>(MeAjudaAi.Shared.Serialization.SerializationDefaults.Default);
        var providerInSearch = searchResult?.Items.FirstOrDefault(p => p.ProviderId == providerId);
        
        providerInSearch.Should().NotBeNull();
        // Nota 5 deve ser a média (já que é a única avaliação)
        providerInSearch!.AverageRating.Should().Be(5.0m);
        providerInSearch.TotalReviews.Should().Be(1);
    }

    private async Task<Guid> CreateTestProviderAsync()
    {
        EventsEnabledTestContainerFixture.AuthenticateAsAdmin();

        var userId = await Fixture.CreateTestUserAsync();
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

        var response = await Fixture.ApiClient.PostAsJsonAsync("/api/v1/providers", request);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString();
        var providerId = TestContainerFixture.ExtractIdFromLocation(location!);

        // Não tentaremos verificar/ativar via REST API porque as regras de estado (PendingBasicInfo -> etc)
        // requerem múltiplos endpoints que não são o foco deste teste.
        // Faremos a inserção direta do provider no SearchProviders (como em SearchProvidersEndToEndTests)
        // com rating = 0 para testar exclusivamente a integração de Reviews -> Search.
        await InsertSearchableProviderAsync(providerId, name, -23.5505, -46.6333);

        return providerId;
    }

    private async Task InsertSearchableProviderAsync(Guid providerId, string name, double latitude, double longitude)
    {
        await Fixture.InsertSearchableProviderAsync(providerId, name, latitude, longitude);
    }
}