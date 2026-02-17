using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Handlers.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.FeatureManagement;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Queries;

[Trait("Category", "Unit")]
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
    public async Task HandleAsync_WhenProviderIsActive_ShouldReturnDtoWithVerificationStatus()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();

        // Bypass domain transitions to set Active status directly for test
        var statusProp = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProp.Should().NotBeNull("Provider.Status property must exist");
        statusProp!.SetValue(provider, EProviderStatus.Active);

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Name.Should().Be(provider.Name);
        result.Value.VerificationStatus.Should().Be(EVerificationStatus.Verified);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderIsNotActive_ShouldReturnNotFound()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .Build(); 
        // Default builder status is PendingBasicInfo (not Active)

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetPublicProviderByIdQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(404);
    }
    [Fact]
    public async Task HandleAsync_WhenPrivacyFlagIsEnabled_ShouldReturnRestrictedProvider()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithBusinessProfile(new BusinessProfile(
                "Restricted Legal", 
                new ContactInfo("privacy@test.com", "11999999999"), 
                new Address("Street", "1", "Neighborhood", "City", "ST", "00000-000", "Country"), 
                "Restricted Fantasy", 
                "Description"))
            .Build();
        
        // Bypass domain transitions to set Active status directly for test
        var statusProp = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProp.Should().NotBeNull("Provider.Status property must exist");
        statusProp!.SetValue(provider, EProviderStatus.Active);
        
        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
            
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.PublicProfilePrivacy))
            .ReturnsAsync(true);

        var query = new GetPublicProviderByIdQuery(provider.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Email.Should().BeNull();
        result.Value.PhoneNumbers.Should().BeEmpty();
        result.Value.Services.Should().BeEmpty();
    }
}
