using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MeAjudaAi.E2E.Tests.Base;
using MeAjudaAi.Modules.SearchProviders.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Contracts.Models;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Handlers;
using Xunit;

namespace MeAjudaAi.E2E.Tests.Modules.Ratings;

[Trait("Category", "E2E")]
[Trait("Module", "Ratings")]
public class RatingsEndToEndTests : BaseTestContainerTest
{
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

        // 3. Aguardar processamento do evento de integração (consistência eventual)
        await Task.Delay(1000);

        // 4. Verificar se a média foi atualizada no módulo de busca
        var searchResponse = await ApiClient.GetAsync($"/api/v1/search/providers?searchTerm={providerId}");
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
    AuthenticateAsAdmin();

    var userId = await CreateTestUserAsync();
    var name = $"Provider_{Guid.NewGuid():N}";
    var request = new
    {
        UserId = userId.ToString(),
        Name = name,
        Type = 0, // Individual
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

    // Verificar o prestador para que ele apareça na busca
    var verifyRequest = new
    {
        Status = 1, // Verified
        Notes = "E2E automatic verification"
    };

    var verifyResponse = await ApiClient.PutAsJsonAsync($"/api/v1/providers/{providerId}/verification-status", verifyRequest);
    verifyResponse.EnsureSuccessStatusCode();

    return providerId;
    }
    }
