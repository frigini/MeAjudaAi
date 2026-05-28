using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Integration tests for Provider persistence with real database (TestContainers).
/// </summary>
public class ProviderPersistenceIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Providers;

    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task Add_WithValidProvider_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        var providerQueries = scope.ServiceProvider.GetRequiredService<IProviderQueries>();
        var repository = uow.GetRepository<Provider, ProviderId>();
        var provider = CreateValidProvider();

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
        var provider = CreateValidProvider(userId);
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
        var provider = CreateValidProvider(userId);
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
        var provider1 = CreateValidProviderWithAddress(city, "SP");
        var provider2 = CreateValidProviderWithAddress(city, "SP");

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

    #region Helper Methods

    private Provider CreateValidProvider(Guid? userId = null, string? city = null, string? state = null)
    {
        var contactInfo = new ContactInfo(
            email: _faker.Internet.Email(),
            phoneNumber: _faker.Phone.PhoneNumber("(##) #####-####"),
            website: null);

        var address = new Address(
            street: _faker.Address.StreetAddress(),
            number: _faker.Random.Number(1, 9999).ToString(),
            neighborhood: _faker.Address.County(),
            city: city ?? _faker.Address.City(),
            state: state ?? _faker.Address.StateAbbr(),
            zipCode: _faker.Address.ZipCode(),
            country: "Brazil",
            complement: null);

        var businessProfile = new BusinessProfile(
            legalName: _faker.Company.CompanyName(),
            contactInfo: contactInfo,
            primaryAddress: address,
            fantasyName: _faker.Company.CompanyName(),
            description: _faker.Company.CatchPhrase());

        return new Provider(
            userId: userId ?? UuidGenerator.NewId(),
            name: _faker.Name.FullName(),
            type: EProviderType.Individual,
            businessProfile: businessProfile);
    }

    private Provider CreateValidProviderWithAddress(string city, string state)
        => CreateValidProvider(city: city, state: state);

    #endregion
}
