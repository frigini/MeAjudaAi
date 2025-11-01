using MeAjudaAi.Modules.Providers.Application.DTOs;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Application.Services;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Shared.Contracts.Modules.Providers;
using MeAjudaAi.Shared.Contracts.Modules.Providers.DTOs;
using MeAjudaAi.Shared.Functional;
using MeAjudaAi.Shared.Queries;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Services;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class ProvidersModuleApiTests
{
    private readonly Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>> _getProviderByIdHandler;
    private readonly Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>> _getProviderByUserIdHandler;
    private readonly Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>> _getProviderByDocumentHandler;
    private readonly Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByIdsHandler;
    private readonly Mock<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByCityHandler;
    private readonly Mock<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByStateHandler;
    private readonly Mock<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByTypeHandler;
    private readonly Mock<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>> _getProvidersByVerificationStatusHandler;
    private readonly Mock<IServiceProvider> _serviceProvider;
    private readonly Mock<ILogger<ProvidersModuleApi>> _logger;
    private readonly ProvidersModuleApi _sut;

    public ProvidersModuleApiTests()
    {
        _getProviderByIdHandler = new Mock<IQueryHandler<GetProviderByIdQuery, Result<ProviderDto?>>>();
        _getProviderByUserIdHandler = new Mock<IQueryHandler<GetProviderByUserIdQuery, Result<ProviderDto?>>>();
        _getProviderByDocumentHandler = new Mock<IQueryHandler<GetProviderByDocumentQuery, Result<ProviderDto?>>>();
        _getProvidersByIdsHandler = new Mock<IQueryHandler<GetProvidersByIdsQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByCityHandler = new Mock<IQueryHandler<GetProvidersByCityQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByStateHandler = new Mock<IQueryHandler<GetProvidersByStateQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByTypeHandler = new Mock<IQueryHandler<GetProvidersByTypeQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _getProvidersByVerificationStatusHandler = new Mock<IQueryHandler<GetProvidersByVerificationStatusQuery, Result<IReadOnlyList<ProviderDto>>>>();
        _serviceProvider = new Mock<IServiceProvider>();
        _logger = new Mock<ILogger<ProvidersModuleApi>>();

        _sut = new ProvidersModuleApi(
            _getProviderByIdHandler.Object,
            _getProviderByUserIdHandler.Object,
            _getProviderByDocumentHandler.Object,
            _getProvidersByIdsHandler.Object,
            _getProvidersByCityHandler.Object,
            _getProvidersByStateHandler.Object,
            _getProvidersByTypeHandler.Object,
            _getProvidersByVerificationStatusHandler.Object,
            _serviceProvider.Object,
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
    public void ApiVersion_ShouldReturn_Version1()
    {
        // Act
        var result = _sut.ApiVersion;

        // Assert
        result.Should().Be("1.0");
    }

    [Fact]
    public async Task IsAvailableAsync_WhenHealthy_ShouldReturn_True()
    {
        // Arrange
        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAvailableAsync_WhenBasicOperationsFail_ShouldReturn_False()
    {
        // Arrange
        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Handler failed"));

        // Act
        var result = await _sut.IsAvailableAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetProviderByIdAsync_WhenProviderExists_ShouldReturnModuleProviderDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateValidProviderDto();

        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerDto.Id);
        result.Value.Name.Should().Be(providerDto.Name);
    }

    [Fact]
    public async Task GetProviderByIdAsync_WhenProviderNotFound_ShouldReturnNull()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    [Fact]
    public async Task GetProviderByIdAsync_WhenHandlerFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var error = Error.Internal("Database error");

        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Failure(error));

        // Act
        var result = await _sut.GetProviderByIdAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public async Task GetProviderByUserIdAsync_WhenProviderExists_ShouldReturnModuleProviderDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerDto = CreateValidProviderDto();

        _getProviderByUserIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.GetProviderByUserIdAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProvidersBatchAsync_WithMultipleProviders_ShouldReturnBasicDtos()
    {
        // Arrange
        var providerIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        var providerDtos = providerIds.Select(_ => CreateValidProviderDto()).ToList();

        _getProvidersByIdsHandler.Setup(x => x.HandleAsync(It.IsAny<GetProvidersByIdsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<IReadOnlyList<ProviderDto>>.Success(providerDtos));

        // Act
        var result = await _sut.GetProvidersBatchAsync(providerIds);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().OnlyContain(x => !string.IsNullOrEmpty(x.Name));
    }

    [Fact]
    public async Task ProviderExistsAsync_WhenProviderExists_ShouldReturnTrue()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var providerDto = CreateValidProviderDto();

        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task ProviderExistsAsync_WhenProviderNotFound_ShouldReturnFalse()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        _getProviderByIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.ProviderExistsAsync(providerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task UserIsProviderAsync_WhenUserIsProvider_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var providerDto = CreateValidProviderDto();

        _getProviderByUserIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(providerDto));

        // Act
        var result = await _sut.UserIsProviderAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task UserIsProviderAsync_WhenUserIsNotProvider_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _getProviderByUserIdHandler.Setup(x => x.HandleAsync(It.IsAny<GetProviderByUserIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProviderDto?>.Success(null));

        // Act
        var result = await _sut.UserIsProviderAsync(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    private static ProviderDto CreateValidProviderDto()
    {
        return new ProviderDto(
            Id: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            Name: "Test Provider",
            Type: MeAjudaAi.Modules.Providers.Domain.Enums.EProviderType.Individual,
            BusinessProfile: new BusinessProfileDto(
                LegalName: "Test Provider Legal",
                FantasyName: "Test Provider Fantasy",
                Description: "Test Description",
                ContactInfo: new ContactInfoDto(
                    Email: "test@example.com",
                    PhoneNumber: "+55 11 99999-9999",
                    Website: "https://test.com"
                ),
                PrimaryAddress: new AddressDto(
                    Street: "Test Street",
                    Number: "123",
                    Complement: "Apt 1",
                    Neighborhood: "Test Neighborhood",
                    City: "Test City",
                    State: "TS",
                    ZipCode: "12345-678",
                    Country: "Brasil"
                )
            ),
            VerificationStatus: MeAjudaAi.Modules.Providers.Domain.Enums.EVerificationStatus.Verified,
            Documents: [],
            Qualifications: [],
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: null,
            IsDeleted: false,
            DeletedAt: null
        );
    }
}
