using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.ModuleApi;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Contracts.Modules.Locations;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Services;

/// <summary>
/// Testes unitários para ProvidersModuleApi
/// </summary>
[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "ModuleApi")]
public class ProvidersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>> _getProviderByIdHandlerMock;
    private readonly Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>> _getProviderByUserIdHandlerMock;
    private readonly Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>> _getProviderByDocumentHandlerMock;
    private readonly Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByIdsHandlerMock;
    private readonly Mock<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByCityHandlerMock;
    private readonly Mock<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByStateHandlerMock;
    private readonly Mock<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByTypeHandlerMock;
    private readonly Mock<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByVerificationStatusHandlerMock;
    private readonly Mock<ILocationsModuleApi> _locationApiMock;
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ILogger<ProvidersModuleApi>> _logger;
    private readonly ProvidersModuleApi _sut;

    public ProvidersModuleApiTests()
    {
        _getProviderByIdHandlerMock = new Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>>();
        _getProviderByUserIdHandlerMock = new Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>>();
        _getProviderByDocumentHandlerMock = new Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>>();
        _getProvidersByIdsHandlerMock = new Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByCityHandlerMock = new Mock<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByStateHandlerMock = new Mock<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByTypeHandlerMock = new Mock<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByVerificationStatusHandlerMock = new Mock<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _locationApiMock = new Mock<ILocationsModuleApi>();
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _logger = new Mock<ILogger<ProvidersModuleApi>>();

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
            _logger.Object);
    }

    [Fact]
    public void ModuleName_ShouldReturn_Providers()
    {
        // Act
        var result = _sut.ModuleName;

        // Assert
        result.Should().Be("Providers");
    }

    [Fact]
    public void ApiVersion_ShouldReturn_1Point0()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    // Note: These tests bypass HealthCheckService by returning null to avoid complex mocking.
    // The health check filtering logic (tags: "providers", "database") is not covered here.
    // Consider adding integration tests for full health check validation.
    [Fact]
    public async Task IsAvailableAsync_WithHealthySystem_ShouldReturnTrue()
    {
        // Arrange - Since HealthCheckService is difficult to mock directly, we'll return null
        // and let the method fall back to its basic operations check only
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        // Setup basic operations test to pass (return Success with null)
        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.IsAny<GetProviderByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WithFailingBasicOperations_ShouldReturnFalse()
    {
        // Arrange - Basic operation fails, indicating system is not available
        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns((HealthCheckService?)null);

        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.IsAny<GetProviderByIdQuery>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(Error.Internal("Database connection failed")));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithExistingProvider_ShouldReturnProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByIdQuery>(q => q.ProviderId == providerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
        result.Value.Name.Should().Be(providerDto.Name);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WithNonExistentProvider_ShouldReturnNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByIdQuery>(q => q.ProviderId == providerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task ProviderExistsAsync_WithExistingProvider_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByIdQuery>(q => q.ProviderId == providerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderExistsAsync_WithNonExistentProvider_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _getProviderByIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByIdQuery>(q => q.ProviderId == providerId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    private static ProviderDto CreateTestProviderDto(Guid id)
    {
        return new ProviderDto(
            Id: id,
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Type: EProviderType.Individual,
            BusinessProfile: new BusinessProfileDto(
                LegalName: "Test Provider Legal Name",
                FantasyName: "Test Provider",
                Description: "Test provider description",
                ContactInfo: new ContactInfoDto(
                    Email: "test@example.com",
                    PhoneNumber: "+5511999999999",
                    Website: "https://test.com"
                ),
                PrimaryAddress: new AddressDto(
                    Street: "Test Street",
                    Number: "123",
                    Complement: null,
                    Neighborhood: "Test Neighborhood",
                    City: "Test City",
                    State: "TS",
                    ZipCode: "12345678",
                    Country: "Brasil"
                )
            ),
            Status: EProviderStatus.PendingBasicInfo,
            VerificationStatus: EVerificationStatus.Pending,
            Tier: EProviderTier.Standard,
            Documents: new List<DocumentDto>
            {
                new DocumentDto(
                    Number: "12345678901",
                    DocumentType: EDocumentType.CPF,
                    IsPrimary: true
                )
            },
            Qualifications: new List<QualificationDto>(),
            Services: new List<ProviderServiceDto>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null,
            SuspensionReason: null,
            RejectionReason: null
        );
    }

    #region Additional Tests for Improved Coverage

    [Fact]
    public async Task GetProviderByDocumentAsync_WithValidDocument_ShouldReturnProvider()
    {
        // Arrange
        var document = "12345678901";
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByDocumentHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByDocumentQuery>(q => q.Document == document),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByDocumentAsync(document);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
    }

    [Fact]
    public async Task GetProviderByUserIdAsync_WithValidUserId_ShouldReturnProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByUserIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);
    }

    [Fact]
    public async Task UserIsProviderAsync_WithExistingUserAsProvider_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByUserIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.UserIsProviderAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UserIsProviderAsync_WithUserNotBeingProvider_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _getProviderByUserIdHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByUserIdQuery>(q => q.UserId == userId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.UserIsProviderAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task DocumentExistsAsync_WithExistingDocument_ShouldReturnTrue()
    {
        // Arrange
        var document = "12345678901";
        var providerId = Guid.NewGuid();
        var providerDto = CreateTestProviderDto(providerId);

        _getProviderByDocumentHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProviderByDocumentQuery>(q => q.Document == document),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.DocumentExistsAsync(document);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task GetProvidersByTypeAsync_WithValidTypeString_ShouldReturnProviders()
    {
        // Arrange
        var typeString = "Individual";
        var providers = new List<ProviderDto>
        {
            CreateTestProviderDto(Guid.NewGuid())
        };

        _getProvidersByTypeHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByTypeQuery>(q => q.Type == EProviderType.Individual),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersByTypeAsync(typeString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProvidersByTypeAsync_WithInvalidTypeString_ShouldReturnFailure()
    {
        // Arrange
        var invalidTypeString = "InvalidType";

        // Act
        var result = await _sut.GetProvidersByTypeAsync(invalidTypeString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProvidersByVerificationStatusAsync_WithValidStatusString_ShouldReturnProviders()
    {
        // Arrange
        var statusString = "Verified";
        var providers = new List<ProviderDto>
        {
            CreateTestProviderDto(Guid.NewGuid())
        };

        _getProvidersByVerificationStatusHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByVerificationStatusQuery>(q => q.Status == EVerificationStatus.Verified),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersByVerificationStatusAsync(statusString);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProvidersByVerificationStatusAsync_WithInvalidStatusString_ShouldReturnFailure()
    {
        // Arrange
        var invalidStatusString = "InvalidStatus";

        // Act
        var result = await _sut.GetProvidersByVerificationStatusAsync(invalidStatusString);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task GetProvidersBasicInfoAsync_WithValidProviderIds_ShouldReturnBasicInfo()
    {
        // Arrange
        var providerIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var providers = providerIds.Select(CreateTestProviderDto).ToList();

        _getProvidersByIdsHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByIdsQuery>(q => q.ProviderIds.SequenceEqual(providerIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersBasicInfoAsync(providerIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProvidersBatchAsync_WithValidProviderIds_ShouldReturnProviders()
    {
        // Arrange
        var providerIds = new[] { Guid.NewGuid(), Guid.NewGuid() };
        var providers = providerIds.Select(CreateTestProviderDto).ToList();

        _getProvidersByIdsHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByIdsQuery>(q => q.ProviderIds.SequenceEqual(providerIds)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersBatchAsync(providerIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetProvidersByCityAsync_WithValidCity_ShouldReturnProviders()
    {
        // Arrange
        var city = "São Paulo";
        var providers = new List<ProviderDto> { CreateTestProviderDto(Guid.NewGuid()) };

        _getProvidersByCityHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByCityQuery>(q => q.City == city),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersByCityAsync(city);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetProvidersByStateAsync_WithValidState_ShouldReturnProviders()
    {
        // Arrange
        var state = "SP";
        var providers = new List<ProviderDto> { CreateTestProviderDto(Guid.NewGuid()) };

        _getProvidersByStateHandlerMock.Setup(x => x.HandleAsync(
                It.Is<GetProvidersByStateQuery>(q => q.State == state),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providers));

        // Act
        var result = await _sut.GetProvidersByStateAsync(state);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion
}
