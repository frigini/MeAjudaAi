using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.ModuleApi;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Services;

[Trait("Category", "Unit")]
public class ProvidersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>> _getProviderByIdHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>> _getProviderByUserIdHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>> _getProviderByDocumentHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByIdsHandlerMock = new();
    private readonly Mock<ILocationsModuleApi> _locationApiMock = new();
    private readonly Mock<IProviderQueries> _providerQueriesMock = new();
    private readonly Mock<ILogger<ProvidersModuleApi>> _loggerMock = new();
    private readonly ProvidersModuleApi _sut;

    public ProvidersModuleApiTests()
    {
        _sut = new ProvidersModuleApi(
            _getProviderByIdHandlerMock.Object,
            _getProviderByUserIdHandlerMock.Object,
            _getProviderByDocumentHandlerMock.Object,
            _getProvidersByIdsHandlerMock.Object,
            _locationApiMock.Object,
            _providerQueriesMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WhenProviderExists_ShouldReturnSuccessWithDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateProviderDto(providerId);
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(It.Is<GetProviderByIdQuery>(q => q.ProviderId == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
    }

    private static ProviderDto CreateProviderDto(Guid providerId)
    {
        return new ProviderDto(
            Id: providerId,
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Slug: "test-provider",
            Type: EProviderType.Individual,
            BusinessProfile: new BusinessProfileDto(
                "Legal Name", null, null,
                new ContactInfoDto("e@e.com", null, null),
                null),
            Status: EProviderStatus.Active,
            VerificationStatus: EVerificationStatus.Verified,
            Tier: EProviderTier.Standard,
            Documents: new List<DocumentDto>(),
            Qualifications: new List<QualificationDto>(),
            Services: new List<ProviderServiceDto>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            IsActive: true);
    }

    #region IsAvailableAsync Tests

    [Fact]
    public async Task IsAvailableAsync_WhenCanConnectReturnsTrue_ShouldReturnTrue()
    {
        // Arrange
        _providerQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeTrue();
        _providerQueriesMock.Verify(x => x.CanConnectAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCanConnectReturnsFalse_ShouldReturnFalse()
    {
        // Arrange
        _providerQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.IsAvailableAsync(default(CancellationToken));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        _providerQueriesMock.Setup(x => x.CanConnectAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _sut.IsAvailableAsync(cts.Token));
    }

    #endregion
}


