using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "CommandHandler")]
public class AddServiceToProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _repositoryMock;
    private readonly Mock<IServiceCatalogsModuleApi> _serviceCatalogsMock;
    private readonly Mock<ILogger<AddServiceToProviderCommandHandler>> _loggerMock;
    private readonly AddServiceToProviderCommandHandler _sut;

    public AddServiceToProviderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProviderRepository>();
        _serviceCatalogsMock = new Mock<IServiceCatalogsModuleApi>();
        _loggerMock = new Mock<ILogger<AddServiceToProviderCommandHandler>>();

        _sut = new AddServiceToProviderCommandHandler(
            _repositoryMock.Object,
            _serviceCatalogsMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidService_ShouldAddServiceToProvider()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: true,
            InvalidServiceIds: Array.Empty<Guid>(),
            InactiveServiceIds: Array.Empty<Guid>());

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        _serviceCatalogsMock
            .Setup(x => x.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceDto?>.Success(new ModuleServiceDto(serviceId, Guid.NewGuid(), "Category", "Test Service", "Description", true)));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Services.Should().ContainSingle(s => s.ServiceId == serviceId);
        _repositoryMock.Verify(x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddServiceToProviderCommand(Guid.NewGuid(), Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Prestador não encontrado");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithServiceValidationFailure_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddServiceToProviderCommand(providerId, serviceId);
        var provider = ProviderBuilder.Create().Build();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Failure("Service validation failed"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Falha ao validar serviço");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidService_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddServiceToProviderCommand(providerId, serviceId);
        var provider = ProviderBuilder.Create().Build();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: false,
            InvalidServiceIds: new[] { serviceId },
            InactiveServiceIds: Array.Empty<Guid>());

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não existe");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithInactiveService_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddServiceToProviderCommand(providerId, serviceId);
        var provider = ProviderBuilder.Create().Build();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: false,
            InvalidServiceIds: Array.Empty<Guid>(),
            InactiveServiceIds: new[] { serviceId });

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("não está ativo");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddServiceToProviderCommand(providerId, serviceId);
        var provider = ProviderBuilder.Create().Build();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: true,
            InvalidServiceIds: Array.Empty<Guid>(),
            InactiveServiceIds: Array.Empty<Guid>());

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        _serviceCatalogsMock
            .Setup(x => x.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceDto?>.Success(new ModuleServiceDto(serviceId, Guid.NewGuid(), "Category", "Test Service", "Description", true)));

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao adicionar serviço ao prestador");
    }

    [Fact]
    public async Task HandleAsync_WhenGetServiceByIdFails_ShouldReturnFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var command = new AddServiceToProviderCommand(providerId, serviceId);

        var provider = ProviderBuilder.Create().Build();

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Service validation succeeds
        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: true,
            InvalidServiceIds: Array.Empty<Guid>(),
            InactiveServiceIds: Array.Empty<Guid>());

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        // GetServiceByIdAsync fails
        _serviceCatalogsMock
            .Setup(x => x.GetServiceByIdAsync(serviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceDto?>.Failure(Error.NotFound("Service not found")));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Falha ao recuperar detalhes do serviço.");
    }
}
