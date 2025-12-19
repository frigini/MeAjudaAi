using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Time;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Providers.Tests.Integration;

/// <summary>
/// Testes de integração para o repositório de prestadores.
/// Valida persistência real com banco de dados PostgreSQL via TestContainers.
/// </summary>
/// <remarks>
/// Verifica operações CRUD do repositório:
/// - Criação e persistência de providers
/// - Consultas por diferentes critérios
/// - Atualizações e deletes (soft e hard)
/// - Relacionamentos com value objects
/// - EF Core mappings e constraints
/// </remarks>
[Collection("ProvidersIntegrationTests")]
public class ProviderRepositoryTests : ProvidersIntegrationTestBase
{
    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldPersistToDatabase()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);
        var dbContext = GetScopedService<ProvidersDbContext>(scope);

        var provider = CreateTestProvider("João Silva", EProviderType.Individual);

        // Act
        await repository.AddAsync(provider);

        // Assert
        var savedProvider = await repository.GetByIdAsync(provider.Id);
        savedProvider.Should().NotBeNull();
        savedProvider!.Name.Should().Be("João Silva");
        savedProvider.Type.Should().Be(EProviderType.Individual);
        savedProvider.BusinessProfile.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider = CreateTestProvider("Maria Santos", EProviderType.Company);
        await repository.AddAsync(provider);

        // Act
        var result = await repository.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
        result.Name.Should().Be("Maria Santos");
        result.Type.Should().Be(EProviderType.Company);
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentProvider_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var nonExistentId = ProviderId.New();

        // Act
        var result = await repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUser_ShouldReturnProvider()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var userId = Guid.NewGuid();
        var provider = CreateTestProvider("Carlos Tech", EProviderType.Company, userId: userId);
        await repository.AddAsync(provider);

        // Act
        var result = await repository.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Name.Should().Be("Carlos Tech");
    }

    [Fact]
    public async Task UpdateAsync_WithValidChanges_ShouldPersistChanges()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider = CreateTestProvider("Old Name", EProviderType.Individual);
        await repository.AddAsync(provider);

        // Act
        var updatedBusinessProfile = new BusinessProfile(
            "New Name Legal",
            new ContactInfo(
                provider.BusinessProfile.ContactInfo.Email,
                provider.BusinessProfile.ContactInfo.PhoneNumber,
                provider.BusinessProfile.ContactInfo.Website),
            new Address(
                provider.BusinessProfile.PrimaryAddress.Street,
                provider.BusinessProfile.PrimaryAddress.Number,
                provider.BusinessProfile.PrimaryAddress.Neighborhood,
                provider.BusinessProfile.PrimaryAddress.City,
                provider.BusinessProfile.PrimaryAddress.State,
                provider.BusinessProfile.PrimaryAddress.ZipCode,
                provider.BusinessProfile.PrimaryAddress.Country,
                provider.BusinessProfile.PrimaryAddress.Complement),
            description: "Updated description");
        provider.UpdateProfile("New Name", updatedBusinessProfile);
        await repository.UpdateAsync(provider);

        // Assert
        var updatedProvider = await repository.GetByIdAsync(provider.Id);
        updatedProvider.Should().NotBeNull();
        updatedProvider!.Name.Should().Be("New Name");
        updatedProvider.BusinessProfile.Description.Should().Be("Updated description");
    }

    [Fact]
    public async Task GetByCityAsync_WithProvidersInCity_ShouldReturnMatching()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider1 = CreateTestProvider("SP Provider 1", EProviderType.Individual, city: "São Paulo");
        var provider2 = CreateTestProvider("RJ Provider", EProviderType.Individual, city: "Rio de Janeiro");
        var provider3 = CreateTestProvider("SP Provider 2", EProviderType.Company, city: "São Paulo");

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);
        await repository.AddAsync(provider3);

        // Act
        var result = await repository.GetByCityAsync("São Paulo");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.BusinessProfile.PrimaryAddress.City == "São Paulo");
    }

    [Fact]
    public async Task GetByStateAsync_WithProvidersInState_ShouldReturnMatching()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider1 = CreateTestProvider("SP Provider 1", EProviderType.Individual, state: "SP");
        var provider2 = CreateTestProvider("RJ Provider", EProviderType.Individual, state: "RJ");
        var provider3 = CreateTestProvider("SP Provider 2", EProviderType.Company, state: "SP");

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);
        await repository.AddAsync(provider3);

        // Act
        var result = await repository.GetByStateAsync("SP");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.BusinessProfile.PrimaryAddress.State == "SP");
    }

    [Fact]
    public async Task GetByTypeAsync_WithProvidersOfType_ShouldReturnMatching()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider1 = CreateTestProvider("Individual 1", EProviderType.Individual);
        var provider2 = CreateTestProvider("Company 1", EProviderType.Company);
        var provider3 = CreateTestProvider("Individual 2", EProviderType.Individual);

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);
        await repository.AddAsync(provider3);

        // Act
        var result = await repository.GetByTypeAsync(EProviderType.Individual);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.Type == EProviderType.Individual);
    }

    [Fact]
    public async Task GetByVerificationStatusAsync_WithProvidersOfStatus_ShouldReturnMatching()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider1 = CreateTestProvider("Pending 1", EProviderType.Individual, status: EVerificationStatus.Pending);
        var provider2 = CreateTestProvider("Verified", EProviderType.Company, status: EVerificationStatus.Verified);
        var provider3 = CreateTestProvider("Pending 2", EProviderType.Individual, status: EVerificationStatus.Pending);

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);
        await repository.AddAsync(provider3);

        // Act
        var result = await repository.GetByVerificationStatusAsync(EVerificationStatus.Pending);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.VerificationStatus == EVerificationStatus.Pending);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingProvider_ShouldReturnTrue()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider = CreateTestProvider("Test Provider", EProviderType.Individual);
        await repository.AddAsync(provider);

        // Act
        var exists = await repository.ExistsAsync(provider.Id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentProvider_ShouldReturnFalse()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var nonExistentId = ProviderId.New();

        // Act
        var exists = await repository.ExistsAsync(nonExistentId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_WithDeletedProvider_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider = CreateTestProvider("Deleted Provider", EProviderType.Individual);
        await repository.AddAsync(provider);

        // Soft delete
        var dateTimeProvider = GetService<IDateTimeProvider>();
        provider.Delete(dateTimeProvider);
        await repository.UpdateAsync(provider);

        // Act
        var result = await repository.GetByIdAsync(provider.Id);

        // Assert
        result.Should().BeNull(); // Soft deleted providers should not be returned
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveProviderFromDatabase()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);
        var dbContext = GetScopedService<ProvidersDbContext>(scope);

        var provider = CreateTestProvider("To Delete", EProviderType.Individual);
        await repository.AddAsync(provider);

        // Verificar que o provider existe
        var providerBeforeDelete = await repository.GetByIdAsync(provider.Id);
        providerBeforeDelete.Should().NotBeNull();

        // Act
        await repository.DeleteAsync(provider.Id);

        // Assert - GetByIdAsync deve retornar null após hard delete
        var deletedProvider = await repository.GetByIdAsync(provider.Id);
        deletedProvider.Should().BeNull();

        // Verificar hard delete no banco de dados (provider não deve existir)
        var providerInDb = await dbContext.Providers
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == provider.Id);

        providerInDb.Should().BeNull(); // Hard delete - completamente removido
    }

    [Fact]
    public async Task GetByIdsAsync_WithMultipleIds_ShouldReturnMatchingProviders()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var provider1 = CreateTestProvider("Provider 1", EProviderType.Individual);
        var provider2 = CreateTestProvider("Provider 2", EProviderType.Company);
        var provider3 = CreateTestProvider("Provider 3", EProviderType.Individual);

        await repository.AddAsync(provider1);
        await repository.AddAsync(provider2);
        await repository.AddAsync(provider3);

        var ids = new List<Guid> { provider1.Id.Value, provider3.Id.Value };

        // Act
        var result = await repository.GetByIdsAsync(ids);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == provider1.Id);
        result.Should().Contain(p => p.Id == provider3.Id);
        result.Should().NotContain(p => p.Id == provider2.Id);
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithExistingUser_ShouldReturnTrue()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var userId = Guid.NewGuid();
        var provider = CreateTestProvider("User Provider", EProviderType.Individual, userId: userId);
        await repository.AddAsync(provider);

        // Act
        var exists = await repository.ExistsByUserIdAsync(userId);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithNonExistentUser_ShouldReturnFalse()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var nonExistentUserId = Guid.NewGuid();

        // Act
        var exists = await repository.ExistsByUserIdAsync(nonExistentUserId);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUser_ShouldReturnNull()
    {
        // Arrange
        await CleanupDatabase();
        using var scope = CreateScope();
        var repository = GetScopedService<IProviderRepository>(scope);

        var nonExistentUserId = Guid.NewGuid();

        // Act
        var result = await repository.GetByUserIdAsync(nonExistentUserId);

        // Assert
        result.Should().BeNull();
    }

    /// <summary>
    /// Helper method to create test providers
    /// </summary>
    private static Provider CreateTestProvider(
        string name,
        EProviderType type,
        EVerificationStatus status = EVerificationStatus.Pending,
        Guid? userId = null,
        string city = "São Paulo",
        string state = "SP")
    {
        var businessProfile = new BusinessProfile(
            legalName: name,
            contactInfo: new ContactInfo(
                email: $"test{Guid.NewGuid():N}@example.com",
                phoneNumber: "+55 11 99999-9999"
            ),
            primaryAddress: new Address(
                street: "Rua Teste",
                number: "123",
                neighborhood: "Centro",
                city: city,
                state: state,
                zipCode: "01234-567",
                country: "Brasil"
            )
        );

        var provider = new Provider(
            userId ?? Guid.NewGuid(),
            name,
            type,
            businessProfile);

        // Apply the requested verification status if different from default (Pending)
        if (status != EVerificationStatus.Pending)
        {
            provider.UpdateVerificationStatus(status, "test-system");
        }

        return provider;
    }
}
