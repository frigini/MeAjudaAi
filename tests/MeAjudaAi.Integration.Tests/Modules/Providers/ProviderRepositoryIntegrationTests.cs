using Bogus;
using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Time;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Providers;

/// <summary>
/// Integration tests for ProviderRepository with real database (TestContainers).
/// Tests actual persistence logic, EF mappings, and database constraints.
/// </summary>
public class ProviderRepositoryIntegrationTests : ApiTestBase
{
    private readonly Faker _faker = new("pt_BR");

    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldPersistToDatabase()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var provider = CreateValidProvider();

        // Act
        await repository.AddAsync(provider);

        // Assert
        var retrieved = await repository.GetByIdAsync(provider.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(provider.Id);
        retrieved.Name.Should().Be(provider.Name);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUserId_ShouldReturnProvider()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var userId = Guid.NewGuid();
        var provider = CreateValidProvider(userId);
        await repository.AddAsync(provider);

        // Act
        var result = await repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithExistingUserId_ShouldReturnTrue()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var userId = Guid.NewGuid();
        var provider = CreateValidProvider(userId);
        await repository.AddAsync(provider);

        // Act
        var exists = await repository.ExistsByUserIdAsync(userId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByCityAsync_WithMatchingCity_ShouldReturnProviders()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var city = "São Paulo";
        var provider1 = CreateValidProviderWithAddress(city, "SP");
        var provider2 = CreateValidProviderWithAddress(city, "SP");

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);

        // Act
        var results = await repository.GetByCityAsync(city);

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
        results.Should().Contain(p => p.Id == provider1.Id);
        results.Should().Contain(p => p.Id == provider2.Id);
    }

    [Fact]
    public async Task GetByStateAsync_WithMatchingState_ShouldReturnProviders()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var state = "SP";
        var provider1 = CreateValidProviderWithAddress("São Paulo", state);
        var provider2 = CreateValidProviderWithAddress("Campinas", state);

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);

        // Act
        var results = await repository.GetByStateAsync(state);

        // Assert
        results.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    // TODO: Fix email constraint issue in database schema
    // [Fact]
    // public async Task UpdateAsync_WithModifiedProvider_ShouldPersistChanges()

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
