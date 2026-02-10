using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using Moq;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;

using Microsoft.FeatureManagement;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers;

public class GetPublicProviderByIdQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly GetPublicProviderByIdQueryHandler _handler;

    public GetPublicProviderByIdQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _handler = new GetPublicProviderByIdQueryHandler(_providerRepositoryMock.Object, _featureManagerMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Provider_When_Found_And_Active()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var provider = new Provider(
            new ProviderId(providerId), 
            userId, 
            "Test Provider", 
            EProviderType.Individual,
            new BusinessProfile(
                "Legal Name",
                new ContactInfo("test@example.com", "123456789"),
                new Address("Street", "123", "Neighborhood", "City", "State", "12345678"),
                "Fantasy Name",
                "Description"
            )
        );
        
        var statusProperty = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProperty.Should().NotBeNull("Status property should be available on Provider for tests");
        statusProperty!.SetValue(provider, EProviderStatus.Active);

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
        result.Value.Name.Should().Be("Test Provider");
        result.Value.FantasyName.Should().Be("Fantasy Name");
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Provider_Does_Not_Exist()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetPublicProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Provider_Is_Not_Active()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var provider = new Provider(
            new ProviderId(providerId), 
            userId, 
            "Test Provider", 
            EProviderType.Individual,
            new BusinessProfile(
                "Legal Name",
                new ContactInfo("test@example.com", "123456789"),
                new Address("Street", "123", "Neighborhood", "City", "State", "12345678"),
                "Fantasy Name",
                "Description"
            )
        );

        // Set status to Suspended using reflection to ensure test isolation from default state
        var statusProperty = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProperty.Should().NotBeNull("Status property should be available on Provider for tests");
        statusProperty!.SetValue(provider, EProviderStatus.Suspended);

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404); // Public endpoint hides inactive providers as Not Found
    }

    [Fact]
    public async Task Handle_Should_Return_Restricted_Provider_When_Privacy_Flag_Is_Enabled()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var provider = new Provider(
            new ProviderId(providerId), 
            userId, 
            "Test Provider", 
            EProviderType.Individual,
            new BusinessProfile(
                "Legal Name",
                new ContactInfo("test@example.com", "123456789"),
                new Address("Street", "123", "Neighborhood", "City", "State", "12345678"),
                "Fantasy Name",
                "Description"
            )
        );
        
        var statusProperty = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProperty!.SetValue(provider, EProviderStatus.Active);

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Enable privacy flag
        _featureManagerMock.Setup(x => x.IsEnabledAsync(MeAjudaAi.Shared.Utilities.Constants.FeatureFlags.PublicProfilePrivacy))
            .ReturnsAsync(true);

        var query = new GetPublicProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Services.Should().BeEmpty();
        result.Value.PhoneNumbers.Should().BeEmpty();
    }
}
