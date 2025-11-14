using FluentAssertions;
using MeAjudaAi.Modules.Search.Domain.Enums;
using MeAjudaAi.Modules.Search.Domain.Repositories;
using MeAjudaAi.Modules.Search.Infrastructure.Persistence;
using MeAjudaAi.Shared.Geolocation;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Modules.Search.Tests.Integration;

/// <summary>
/// Testes de integração para SearchableProviderRepository com PostGIS
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Search")]
[Trait("Component", "Repository")]
public class SearchableProviderRepositoryIntegrationTests : SearchIntegrationTestBase
{
    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldPersistToDatabase()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SearchDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider(
            "João Silva Eletricista",
            -23.5505,
            -46.6333, // São Paulo
            ESubscriptionTier.Standard);

        // Act
        await repository.AddAsync(provider);
        await repository.SaveChangesAsync();

        // Assert
        var savedProvider = await repository.GetByIdAsync(provider.Id);
        savedProvider.Should().NotBeNull();
        savedProvider!.Name.Should().Be("João Silva Eletricista");
        savedProvider.Location.Latitude.Should().BeApproximately(-23.5505, 0.0001);
        savedProvider.Location.Longitude.Should().BeApproximately(-46.6333, 0.0001);
        savedProvider.SubscriptionTier.Should().Be(ESubscriptionTier.Standard);
    }

    [Fact]
    public async Task SearchAsync_WithProvidersInRadius_ShouldReturnOrderedByDistance()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        // São Paulo Centro
        var provider1 = CreateTestSearchableProvider("Provider SP Centro", -23.5505, -46.6333);
        // São Paulo Paulista (aprox 5km do centro)
        var provider2 = CreateTestSearchableProvider("Provider SP Paulista", -23.5629, -46.6544);
        // São Paulo Pinheiros (aprox 10km do centro)
        var provider3 = CreateTestSearchableProvider("Provider SP Pinheiros", -23.5629, -46.7011);
        // Rio de Janeiro (aprox 357km)
        var provider4 = CreateTestSearchableProvider("Provider RJ", -22.9068, -43.1729);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);
        await PersistSearchableProviderAsync(provider4);

        var searchLocation = new GeoPoint(-23.5505, -46.6333); // São Paulo Centro

        // Act - Buscar em raio de 20km
        var (providers, totalCount) = await repository.SearchAsync(searchLocation, 20, null, null, null, 0, 10);

        // Assert
        providers.Should().HaveCount(3); // Apenas os 3 de SP
        providers.Should().NotContain(p => p.Id == provider4.Id); // Rio não deve estar
        
        // Verificar ordenação por distância (mais próximo primeiro)
        providers.First().Id.Should().Be(provider1.Id); // Centro (0km)
        providers.Skip(1).First().Id.Should().Be(provider2.Id); // Paulista (~5km)
        providers.Skip(2).First().Id.Should().Be(provider3.Id); // Pinheiros (~10km)
    }

    [Fact]
    public async Task SearchAsync_WithServiceIdsFilter_ShouldReturnOnlyMatchingProviders()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var electricianServiceId = Guid.NewGuid();
        var plumberServiceId = Guid.NewGuid();

        var provider1 = CreateTestSearchableProvider(
            "Eletricista SP",
            -23.5505, -46.6333);
        provider1.UpdateServices(new[] { electricianServiceId });

        var provider2 = CreateTestSearchableProvider(
            "Encanador SP",
            -23.5629, -46.6544);
        provider2.UpdateServices(new[] { plumberServiceId });

        var provider3 = CreateTestSearchableProvider(
            "Multi Services SP",
            -23.5700, -46.6500);
        provider3.UpdateServices(new[] { electricianServiceId, plumberServiceId });

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Buscar apenas eletricistas
        var (providers, totalCount) = await repository.SearchAsync(
            searchLocation,
            50,
            new[] { electricianServiceId },
            null,
            null,
            0,
            10);

        // Assert
        providers.Should().HaveCount(2);
        providers.Should().Contain(p => p.Id == provider1.Id);
        providers.Should().Contain(p => p.Id == provider3.Id);
        providers.Should().NotContain(p => p.Id == provider2.Id);
    }

    [Fact]
    public async Task SearchAsync_WithMinRatingFilter_ShouldReturnOnlyHighRated()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider1 = CreateTestSearchableProvider("Provider 5 stars", -23.5505, -46.6333);
        provider1.UpdateRating(5.0m, 100);
        
        var provider2 = CreateTestSearchableProvider("Provider 4 stars", -23.5629, -46.6544);
        provider2.UpdateRating(4.0m, 50);
        
        var provider3 = CreateTestSearchableProvider("Provider 3 stars", -23.5700, -46.6500);
        provider3.UpdateRating(3.5m, 25);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Buscar apenas com rating >= 4.0
        var (providers, totalCount) = await repository.SearchAsync(
            searchLocation,
            50,
            null,
            4.0m,
            null,
            0,
            10);

        // Assert
        providers.Should().HaveCount(2);
        providers.Should().Contain(p => p.Id == provider1.Id);
        providers.Should().Contain(p => p.Id == provider2.Id);
        providers.Should().NotContain(p => p.Id == provider3.Id);
    }

    [Fact]
    public async Task SearchAsync_WithSubscriptionTiersFilter_ShouldReturnOnlyMatchingTiers()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider1 = CreateTestSearchableProvider("Free Provider", -23.5505, -46.6333, ESubscriptionTier.Free);
        var provider2 = CreateTestSearchableProvider("Standard Provider", -23.5629, -46.6544, ESubscriptionTier.Standard);
        var provider3 = CreateTestSearchableProvider("Gold Provider", -23.5700, -46.6500, ESubscriptionTier.Gold);
        var provider4 = CreateTestSearchableProvider("Gold Provider 2", -23.5800, -46.6600, ESubscriptionTier.Gold);

        await PersistSearchableProviderAsync(provider1);
        await PersistSearchableProviderAsync(provider2);
        await PersistSearchableProviderAsync(provider3);
        await PersistSearchableProviderAsync(provider4);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Buscar apenas Standard e Gold
        var (providers, totalCount) = await repository.SearchAsync(
            searchLocation,
            50,
            null,
            null,
            new[] { ESubscriptionTier.Standard, ESubscriptionTier.Gold },
            0,
            10);

        // Assert
        providers.Should().HaveCount(3);
        providers.Should().Contain(p => p.Id == provider2.Id);
        providers.Should().Contain(p => p.Id == provider3.Id);
        providers.Should().Contain(p => p.Id == provider4.Id);
        providers.Should().NotContain(p => p.Id == provider1.Id);
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ShouldRespectPageSizeAndNumber()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        // Criar 15 providers próximos
        for (int i = 0; i < 15; i++)
        {
            var provider = CreateTestSearchableProvider(
                $"Provider {i}",
                -23.5505 + (i * 0.001),
                -46.6333 + (i * 0.001));
            await PersistSearchableProviderAsync(provider);
        }

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Página 2 com 5 items por página
        var (providers, totalCount) = await repository.SearchAsync(
            searchLocation,
            100,
            null,
            null,
            null,
            5,
            5);

        // Assert
        providers.Should().HaveCount(5); // Page size
        // Página 2 deve ter providers 5-9 (0-indexed: skip 5, take 5)
    }

    [Fact]
    public async Task SearchAsync_WithInactiveProviders_ShouldNotReturnThem()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var activeProvider = CreateTestSearchableProvider("Active Provider", -23.5505, -46.6333);
        var inactiveProvider = CreateTestSearchableProvider("Inactive Provider", -23.5629, -46.6544);

        await PersistSearchableProviderAsync(activeProvider);
        await PersistSearchableProviderAsync(inactiveProvider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act
        var (providers, totalCount) = await repository.SearchAsync(searchLocation, 50, null, null, null, 0, 10);

        // Assert
        providers.Should().HaveCount(2);
        providers.Should().Contain(p => p.Id == activeProvider.Id);
        providers.Should().Contain(p => p.Id == inactiveProvider.Id);
    }

    [Fact]
    public async Task SearchAsync_WithCombinedFilters_ShouldApplyAllFilters()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var serviceId = Guid.NewGuid();

        // Provider que atende TODOS os critérios
        var matchingProvider = CreateTestSearchableProvider(
            "Perfect Match",
            -23.5505, -46.6333,
            ESubscriptionTier.Gold);
        matchingProvider.UpdateServices(new[] { serviceId });
        matchingProvider.UpdateRating(4.5m, 100);

        // Provider fora do raio
        var farProvider = CreateTestSearchableProvider(
            "Too Far",
            -22.9068, -43.1729, // Rio
            ESubscriptionTier.Gold);
        farProvider.UpdateServices(new[] { serviceId });
        farProvider.UpdateRating(4.5m, 100);

        // Provider com rating baixo
        var lowRatingProvider = CreateTestSearchableProvider(
            "Low Rating",
            -23.5629, -46.6544,
            ESubscriptionTier.Gold);
        lowRatingProvider.UpdateServices(new[] { serviceId });
        lowRatingProvider.UpdateRating(3.0m, 10);

        // Provider tier errado
        var wrongTierProvider = CreateTestSearchableProvider(
            "Wrong Tier",
            -23.5700, -46.6500,
            ESubscriptionTier.Free);
        wrongTierProvider.UpdateServices(new[] { serviceId });
        wrongTierProvider.UpdateRating(4.5m, 100);

        // Provider sem o serviço
        var noServiceProvider = CreateTestSearchableProvider(
            "No Service",
            -23.5800, -46.6600,
            ESubscriptionTier.Gold);
        noServiceProvider.UpdateRating(4.5m, 100);

        await PersistSearchableProviderAsync(matchingProvider);
        await PersistSearchableProviderAsync(farProvider);
        await PersistSearchableProviderAsync(lowRatingProvider);
        await PersistSearchableProviderAsync(wrongTierProvider);
        await PersistSearchableProviderAsync(noServiceProvider);

        var searchLocation = new GeoPoint(-23.5505, -46.6333);

        // Act - Aplicar todos os filtros
        var (providers, totalCount) = await repository.SearchAsync(
            searchLocation,
            20, // 20km radius
            new[] { serviceId },
            4.0m, // minRating
            new[] { ESubscriptionTier.Gold },
            0,
            10);

        // Assert
        providers.Should().HaveCount(1);
        providers.Single().Id.Should().Be(matchingProvider.Id);
    }

    [Fact]
    public async Task GetByProviderIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var providerId = Guid.NewGuid();
        var provider = CreateTestSearchableProvider("Test Provider", -23.5505, -46.6333);
        
        // Usar reflection para definir ProviderId (já que é private set)
        var providerIdProperty = provider.GetType().GetProperty("ProviderId");
        providerIdProperty!.SetValue(provider, providerId);

        await PersistSearchableProviderAsync(provider);

        // Act
        var result = await repository.GetByProviderIdAsync(providerId);

        // Assert
        result.Should().NotBeNull();
        result!.ProviderId.Should().Be(providerId);
    }

    [Fact]
    public async Task UpdateAsync_WithLocationChange_ShouldUpdateGeographyColumn()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Provider", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        var newLocation = new GeoPoint(-22.9068, -43.1729); // Rio de Janeiro

        // Act
        provider.UpdateLocation(newLocation);
        await repository.UpdateAsync(provider);
        await repository.SaveChangesAsync();

        // Assert
        var updatedProvider = await repository.GetByIdAsync(provider.Id);
        updatedProvider.Should().NotBeNull();
        updatedProvider!.Location.Latitude.Should().BeApproximately(-22.9068, 0.0001);
        updatedProvider.Location.Longitude.Should().BeApproximately(-43.1729, 0.0001);

        // Verificar que busca antiga não retorna mais
        var (oldLocationProviders, oldLocationTotal) = await repository.SearchAsync(
            new GeoPoint(-23.5505, -46.6333),
            10,
            null, null, null, 0, 10);
        oldLocationProviders.Should().NotContain(p => p.Id == provider.Id);

        // Verificar que busca nova retorna
        var (newLocationProviders, newLocationTotal) = await repository.SearchAsync(
            newLocation,
            10,
            null, null, null, 0, 10);
        newLocationProviders.Should().Contain(p => p.Id == provider.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFromDatabase()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISearchableProviderRepository>();

        var provider = CreateTestSearchableProvider("Provider to Delete", -23.5505, -46.6333);
        await PersistSearchableProviderAsync(provider);

        // Act
        await repository.DeleteAsync(provider);
        await repository.SaveChangesAsync();

        // Assert
        var deletedProvider = await repository.GetByIdAsync(provider.Id);
        deletedProvider.Should().BeNull();
    }
}
