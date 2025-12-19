using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Persistence;

/// <summary>
/// Unit tests for IProviderRepository interface contract validation.
/// Note: These tests use mocks to verify interface behavior contracts,
/// not the concrete ProviderRepository implementation.
/// 
/// For real persistence behavior tests, see:
/// - tests/MeAjudaAi.Integration.Tests/Modules/Providers/ProviderRepositoryIntegrationTests.cs
/// 
/// These unit tests remain useful for:
/// - Fast execution without database dependencies
/// - Verifying repository interface contracts
/// - Testing error handling and edge cases
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Infrastructure")]
public class ProviderRepositoryTests
{
    private readonly Mock<IProviderRepository> _mockRepository;

    public ProviderRepositoryTests()
    {
        _mockRepository = new Mock<IProviderRepository>();
    }

    [Fact]
    public async Task AddAsync_WithValidProvider_ShouldCallRepositoryMethod()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithType(EProviderType.Individual)
            .WithName("Test Provider")
            .Build();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.AddAsync(provider);

        // Assert
        _mockRepository.Verify(x => x.AddAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithType(EProviderType.Company)
            .WithName("Company Provider")
            .Build();

        _mockRepository
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(provider.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(provider.Id);
        result.Name.Should().Be("Company Provider");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentProvider_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = new ProviderId(Guid.NewGuid());

        _mockRepository
            .Setup(x => x.GetByIdAsync(nonExistentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _mockRepository.Object.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdsAsync_WithMultipleIds_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("Provider 1").Build();
        var provider2 = new ProviderBuilder().WithName("Provider 2").Build();
        var ids = new List<Guid> { provider1.Id.Value, provider2.Id.Value };
        var providers = new List<Provider> { provider1, provider2 };

        _mockRepository
            .Setup(x => x.GetByIdsAsync(ids, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _mockRepository.Object.GetByIdsAsync(ids);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Id == provider1.Id);
        result.Should().Contain(p => p.Id == provider2.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithExistingUserId_ShouldReturnProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithUserId(userId)
            .WithName("User Provider")
            .Build();

        _mockRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _mockRepository.Object.GetByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
    }

    [Fact]
    public async Task GetByUserIdAsync_WithNonExistentUserId_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _mockRepository.Object.GetByUserIdAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_WithValidProvider_ShouldCallRepositoryMethod()
    {
        // Arrange
        var provider = new ProviderBuilder()
            .WithName("Updated Provider")
            .Build();

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.UpdateAsync(provider);

        // Assert
        _mockRepository.Verify(x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WithValidId_ShouldCallRepositoryMethod()
    {
        // Arrange
        var providerId = new ProviderId(Guid.NewGuid());

        _mockRepository
            .Setup(x => x.DeleteAsync(providerId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _mockRepository.Object.DeleteAsync(providerId);

        // Assert
        _mockRepository.Verify(x => x.DeleteAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithExistingUserId_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockRepository.Object.ExistsByUserIdAsync(userId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByUserIdAsync_WithNonExistentUserId_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.ExistsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _mockRepository.Object.ExistsByUserIdAsync(userId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetByVerificationStatusAsync_WithPendingStatus_ShouldReturnProviders()
    {
        // Arrange
        var provider1 = new ProviderBuilder().WithName("Pending Provider 1").Build();
        var provider2 = new ProviderBuilder().WithName("Pending Provider 2").Build();
        var providers = new List<Provider> { provider1, provider2 };

        _mockRepository
            .Setup(x => x.GetByVerificationStatusAsync(EVerificationStatus.Pending, It.IsAny<CancellationToken>()))
            .ReturnsAsync(providers);

        // Act
        var result = await _mockRepository.Object.GetByVerificationStatusAsync(EVerificationStatus.Pending);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByVerificationStatusAsync_WithNoProvidersInStatus_ShouldReturnEmpty()
    {
        // Arrange
        _mockRepository
            .Setup(x => x.GetByVerificationStatusAsync(EVerificationStatus.Rejected, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Provider>());

        // Act
        var result = await _mockRepository.Object.GetByVerificationStatusAsync(EVerificationStatus.Rejected);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}
