using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using DomainValueObjects = MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Application.ModuleApi;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Contracts.Modules.Locations.DTOs;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

// Aliases para evitar ambiguidade
using ProviderDto = MeAjudaAi.Modules.Providers.Application.DTOs.ProviderDto;
using BusinessProfileDto = MeAjudaAi.Modules.Providers.Application.DTOs.BusinessProfileDto;
using ContactInfoDto = MeAjudaAi.Modules.Providers.Application.DTOs.ContactInfoDto;
using DocumentDto = MeAjudaAi.Modules.Providers.Application.DTOs.DocumentDto;
using QualificationDto = MeAjudaAi.Modules.Providers.Application.DTOs.QualificationDto;
using ProviderServiceDto = MeAjudaAi.Modules.Providers.Application.DTOs.ProviderServiceDto;
using ModuleProviderDto = MeAjudaAi.Contracts.Modules.Providers.DTOs.ModuleProviderDto;
using ModuleProviderBasicDto = MeAjudaAi.Contracts.Modules.Providers.DTOs.ModuleProviderBasicDto;
using ModuleProviderIndexingDto = MeAjudaAi.Contracts.Modules.Providers.DTOs.ModuleProviderIndexingDto;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Services;

[Trait("Category", "Unit")]
public class ProvidersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>> _getProviderByIdHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>> _getProviderByUserIdHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>> _getProviderByDocumentHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByIdsHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByCityHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByStateHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByTypeHandlerMock = new();
    private readonly Mock<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByVerificationStatusHandlerMock = new();
    private readonly Mock<ILocationsModuleApi> _locationApiMock = new();
    private readonly Mock<IProviderRepository> _providerRepositoryMock = new();
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();
    private readonly Mock<ILogger<ProvidersModuleApi>> _loggerMock = new();
    private readonly ProvidersModuleApi _sut;

    public ProvidersModuleApiTests()
    {
        _sut = new ProvidersModuleApi(
            _getProviderByIdHandlerMock.Object,
            _getProviderByUserIdHandlerMock.Object,
            _getProviderByDocumentHandlerMock.Object,
            _getProvidersByIdsHandlerMock.Object,
            _getProvidersByCityHandlerMock.Object,
            _getProvidersByStateHandlerMock.Object,
            _getProvidersByTypeHandlerMock.Object,
            _getProvidersByVerificationStatusHandlerMock.Object,
            _locationApiMock.Object,
            _providerRepositoryMock.Object,
            _serviceProviderMock.Object,
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

    [Fact]
    public async Task GetProviderByIdAsync_WhenProviderNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(Error.NotFound("Not Found")));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderByUserIdAsync_WhenProviderExists_ShouldReturnSuccessWithDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var providerDto = CreateProviderDto(providerId, userId);
        _getProviderByUserIdHandlerMock.Setup(x => x.HandleAsync(It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
    }

    [Fact]
    public async Task ProviderExistsAsync_WhenProviderExists_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateProviderDto(providerId);
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderExistsAsync_WhenProviderDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(Error.NotFound("Not Found")));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task GetProviderForIndexingAsync_WhenProviderExistsAndGeocodingSucceeds_ShouldReturnIndexingDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = CreateTestProvider(providerId);
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var coordinates = new ModuleCoordinatesDto(10.0, 20.0);
        _locationApiMock.Setup(x => x.GetCoordinatesFromAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleCoordinatesDto>.Success(coordinates));

        // Act
        var result = await _sut.GetProviderForIndexingAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ProviderId.Should().Be(providerId);
        result.Value!.Latitude.Should().Be(10.0);
        result.Value!.Longitude.Should().Be(20.0);
    }

    [Fact]
    public async Task GetProviderForIndexingAsync_WhenProviderNotFound_ShouldReturnSuccessWithNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.GetProviderForIndexingAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderForIndexingAsync_WhenGeocodingFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = CreateTestProvider(providerId);
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _locationApiMock.Setup(x => x.GetCoordinatesFromAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleCoordinatesDto>.Failure(Error.BadRequest("Geocoding failed")));

        // Act
        var result = await _sut.GetProviderForIndexingAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task GetProviderForIndexingAsync_WhenExceptionOccurs_ShouldReturnFailureWithCentralizedMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.GetProviderForIndexingAsync(providerId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(ProvidersErrorMessages.IndexingDataError);
    }

    [Fact]
    public async Task HasProvidersOfferingServiceAsync_ShouldReturnRepositoryResult()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.HasProvidersWithServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.HasProvidersOfferingServiceAsync(serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task HasProvidersOfferingServiceAsync_WhenExceptionOccurs_ShouldReturnFailureWithCentralizedMessage()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.HasProvidersWithServiceAsync(serviceId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.HasProvidersOfferingServiceAsync(serviceId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(ProvidersErrorMessages.ServiceProvidersCheckError);
    }

    [Fact]
    public async Task IsServiceOfferedByProviderAsync_WhenProviderExists_ShouldReturnResultFromEntity()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var provider = CreateTestProvider(providerId);
        provider.AddService(serviceId, "Service");

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.IsServiceOfferedByProviderAsync(providerId, serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task IsServiceOfferedByProviderAsync_WhenProviderNotFound_ShouldReturnSuccessFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.IsServiceOfferedByProviderAsync(providerId, serviceId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task IsServiceOfferedByProviderAsync_WhenExceptionOccurs_ShouldReturnFailureWithCentralizedMessage()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        _providerRepositoryMock.Setup(x => x.GetByIdAsync(new DomainValueObjects.ProviderId(providerId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.IsServiceOfferedByProviderAsync(providerId, serviceId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error!.Message.Should().Be(ProvidersErrorMessages.ProviderServiceCheckError);
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthChecksPass_ShouldReturnTrue()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var healthReport = new HealthReport(new Dictionary<string, HealthReportEntry>(), HealthStatus.Healthy, TimeSpan.FromMilliseconds(100));
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        // Simula CanExecuteBasicOperationsAsync via GetProviderByIdAsync
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthChecksFail_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry> { { "db", new HealthReportEntry(HealthStatus.Unhealthy, "error", TimeSpan.Zero, null, null) } },
            HealthStatus.Unhealthy,
            TimeSpan.FromMilliseconds(100));
        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(It.IsAny<Func<HealthCheckRegistration, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    private static Provider CreateTestProvider(Guid id)
    {
        var address = new DomainValueObjects.Address("Street", "123", "Neighborhood", "City", "ST", "12345678");
        var contactInfo = new DomainValueObjects.ContactInfo("test@test.com");
        var profile = new DomainValueObjects.BusinessProfile("Test Provider", contactInfo, address);
        
        var provider = new Provider(
            new DomainValueObjects.ProviderId(id),
            Guid.NewGuid(),
            "Test Provider",
            EProviderType.Individual,
            profile);

        return provider;
    }

    private static ProviderDto CreateProviderDto(Guid id, Guid? userId = null)
    {
        return new ProviderDto(
            id,
            userId ?? Guid.NewGuid(),
            "Test Provider",
            "test-provider",
            EProviderType.Individual,
            new BusinessProfileDto(
                LegalName: "Test Provider", 
                FantasyName: null, 
                Description: "Description", 
                ContactInfo: new ContactInfoDto("test@test.com", null, null, null), 
                PrimaryAddress: null),
            EProviderStatus.Active,
            EVerificationStatus.Verified,
            EProviderTier.Standard,
            [],
            [],
            [],
            DateTime.UtcNow,
            null,
            false,
            null,
            true,
            null,
            null);
    }
}
