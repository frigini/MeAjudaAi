using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.ModuleApi;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
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

    [Fact]
    public async Task IsAvailableAsync_WithHealthySystem_ShouldReturnTrue()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>(),
            HealthStatus.Healthy,
            TimeSpan.FromMilliseconds(100));

        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(
                It.IsAny<Func<HealthCheckRegistration, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

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
    public async Task IsAvailableAsync_WithUnhealthySystem_ShouldReturnFalse()
    {
        // Arrange
        var healthCheckServiceMock = new Mock<HealthCheckService>();
        var healthReport = new HealthReport(
            new Dictionary<string, HealthReportEntry>
            {
                ["database"] = new HealthReportEntry(
                    HealthStatus.Unhealthy,
                    "Database connection failed",
                    TimeSpan.FromMilliseconds(100),
                    null,
                    null)
            },
            HealthStatus.Unhealthy,
            TimeSpan.FromMilliseconds(100));

        healthCheckServiceMock.Setup(x => x.CheckHealthAsync(
                It.IsAny<Func<HealthCheckRegistration, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(healthReport);

        _serviceProviderMock.Setup(x => x.GetService(typeof(HealthCheckService)))
            .Returns(healthCheckServiceMock.Object);

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
                It.IsAny<GetProviderByIdQuery>(),
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
                It.IsAny<GetProviderByIdQuery>(),
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
            VerificationStatus: EVerificationStatus.Pending,
            Documents: new List<DocumentDto>
            {
                new DocumentDto(
                    Number: "12345678901",
                    DocumentType: EDocumentType.CPF,
                    IsPrimary: true
                )
            },
            Qualifications: new List<QualificationDto>(),
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null
        );
    }
}
