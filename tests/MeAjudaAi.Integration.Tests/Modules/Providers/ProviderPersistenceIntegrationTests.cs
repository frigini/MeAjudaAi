using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Testes de integração para persistência de Provider com banco de dados real (TestContainers).
/// </summary>
public class ProviderPersistenceIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers;

    [Fact]
    public async Task Add_WithValidProvider_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var providerQueries = scope.ServiceProvider.GetRequiredService<IProviderQueries>();
        var repository = uow.GetRepository<Provider, ProviderId>();
        var provider = ProviderBuilder.CreateValid().Build();

        // Act
        repository.Add(provider);
        await uow.SaveChangesAsync();

        // Assert
        var retrieved = await providerQueries.GetByIdAsync(provider.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(provider.Id);
        retrieved.Name.Should().Be(provider.Name);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUserId_ShouldReturnProvider()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var providerQueries = scope.ServiceProvider.GetRequiredService<IProviderQueries>();
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.CreateValid().WithUserId(userId).Build();
        uow.GetRepository<Provider, ProviderId>().Add(provider);
        await uow.SaveChangesAsync();

        // Act
        var result = await providerQueries.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithExistingUserId_ShouldReturnTrue()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var providerQueries = scope.ServiceProvider.GetRequiredService<IProviderQueries>();
        var userId = Guid.NewGuid();
        var provider = ProviderBuilder.CreateValid().WithUserId(userId).Build();
        uow.GetRepository<Provider, ProviderId>().Add(provider);
        await uow.SaveChangesAsync();

        // Act
        var exists = await providerQueries.ExistsByUserIdAsync(userId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCityAsync_WithMatchingCity_ShouldReturnProviders()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var providerQueries = scope.ServiceProvider.GetRequiredService<IProviderQueries>();
        var repository = uow.GetRepository<Provider, ProviderId>();
        var city = "São Paulo";
        var provider1 = ProviderBuilder.CreateValid().WithCity(city).WithState("SP").Build();
        var provider2 = ProviderBuilder.CreateValid().WithCity(city).WithState("SP").Build();

        repository.Add(provider1);
        repository.Add(provider2);
        await uow.SaveChangesAsync();

        // Act
        var results = await providerQueries.GetByCityAsync(city);

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().Contain(p => p.Id == provider1.Id);
        results.Should().Contain(p => p.Id == provider2.Id);
    }
}