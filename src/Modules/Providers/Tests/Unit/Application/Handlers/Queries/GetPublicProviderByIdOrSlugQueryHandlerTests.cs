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
public class GetPublicProviderByIdOrSlugQueryHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IFeatureManager> _featureManagerMock;
    private readonly GetPublicProviderByIdOrSlugQueryHandler _handler;

    public GetPublicProviderByIdOrSlugQueryHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _featureManagerMock = new Mock<IFeatureManager>();
        _handler = new GetPublicProviderByIdOrSlugQueryHandler(_providerRepositoryMock.Object, _featureManagerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderIsActive_ShouldReturnDtoWithVerificationStatus()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();

        // Ignora transições de domínio para definir o status como Active diretamente no teste
        SetProviderStatus(provider, EProviderStatus.Active);

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString());

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
    public async Task HandleAsync_WhenProviderQueriedBySlug_ShouldReturnDto()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();

        // Bypass domain transitions to set Active status directly for test
        SetProviderStatus(provider, EProviderStatus.Active);

        var normalizedSlug = provider.Slug.Trim().ToLowerInvariant();

        _providerRepositoryMock
            .Setup(x => x.GetBySlugAsync(normalizedSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdOrSlugQuery(provider.Slug);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Slug.Should().Be(provider.Slug);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderQueriedByUpperCaseSlug_ShouldNormalizeAndReturnDto()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();

        // Bypass domain transitions to set Active status directly for test
        SetProviderStatus(provider, EProviderStatus.Active);

        var normalizedSlug = provider.Slug.Trim().ToLowerInvariant();
        var upperSlug = provider.Slug.ToUpperInvariant();

        _providerRepositoryMock
            .Setup(x => x.GetBySlugAsync(normalizedSlug, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Query usa slug em maiúsculas — o handler deve normalizar antes de chamar GetBySlugAsync
        var query = new GetPublicProviderByIdOrSlugQuery(upperSlug);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Slug.Should().Be(provider.Slug);
    }

    [Fact]
    public async Task HandleAsync_WhenSlugIsValidGuidString_ShouldFallbackToSlugLookup()
    {
        // Arrange — slug que possui um formato de GUID válido (Guid.TryParse retorna true)
        var slugGuid = Guid.NewGuid();
        var slugValue = slugGuid.ToString().ToLowerInvariant(); // e.g. "3fa85f64-5717-4562-b3fc-2c963f66afa6"

        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .WithVerificationStatus(EVerificationStatus.Verified)
            .Build();

        // Bypass domain transitions to set Active status directly for test
        SetProviderStatus(provider, EProviderStatus.Active);

        // GetByIdAsync retorna null (nenhum provedor com esse ID)
        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Fallback para a busca por slug retorna o provedor
        _providerRepositoryMock
            .Setup(x => x.GetBySlugAsync(slugValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdOrSlugQuery(slugValue);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderIsNotActive_ShouldReturnNotFound()
    {
        // Arrange
        var provider = ProviderBuilder.Create()
            .Build(); 
        // O status padrão do builder é PendingBasicInfo (não Active)

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString());

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
            .Setup(x => x.GetByIdAsync(new ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        var query = new GetPublicProviderByIdOrSlugQuery(providerId.ToString());

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
        SetProviderStatus(provider, EProviderStatus.Active);
        
        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
            
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.PublicProfilePrivacy))
            .ReturnsAsync(true);

        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString()) { IsAuthenticated = true };

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

    [Fact]
    public async Task HandleAsync_WhenAuthenticatedAndPrivacyFlagDisabled_ShouldReturnFullContactInfo()
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
        
        var expectedServiceId = Guid.NewGuid();
        provider.AddService(expectedServiceId, "Known Service");

        SetProviderStatus(provider, EProviderStatus.Active);
        
        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(provider.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);
            
        _featureManagerMock
            .Setup(x => x.IsEnabledAsync(FeatureFlags.PublicProfilePrivacy))
            .ReturnsAsync(false);

        var query = new GetPublicProviderByIdOrSlugQuery(provider.Id.Value.ToString()) { IsAuthenticated = true };

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(provider.Id);
        result.Value.Email.Should().Be("privacy@test.com");
        result.Value.PhoneNumbers.Should().NotBeEmpty();
        result.Value.Services.Should().Contain(s => s.ServiceId == expectedServiceId);
    }

    private static void SetProviderStatus(Provider provider, EProviderStatus status)
    {
        var statusProp = typeof(Provider).GetProperty(nameof(Provider.Status));
        statusProp.Should().NotBeNull("Provider.Status property must exist");
        statusProp!.SetValue(provider, status);
    }
}
