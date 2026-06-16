using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Providers;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class AddServiceToProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IServiceCatalogsModuleApi> _serviceCatalogsMock;
    private readonly Mock<ILogger<AddServiceToProviderCommandHandler>> _loggerMock;
    private readonly AddServiceToProviderCommandHandler _handler;

    public AddServiceToProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _serviceCatalogsMock = new Mock<IServiceCatalogsModuleApi>();
        _loggerMock = new Mock<ILogger<AddServiceToProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new AddServiceToProviderCommandHandler(
            _uowMock.Object,
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

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
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
            .ReturnsAsync(Result<ModuleServiceDto?>.Success(new ModuleServiceDto(
                Id: serviceId,
                ProviderId: Guid.Empty,
                CategoryId: Guid.NewGuid(),
                CategoryName: "Category",
                Name: "Test Service",
                Description: "Description",
                IsActive: true)));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Services.Should().ContainSingle(s => s.ServiceId == serviceId);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new AddServiceToProviderCommand(Guid.NewGuid(), Guid.NewGuid());

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Prestador não encontrado");
        _serviceCatalogsMock.Verify(
            x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceValidationFails_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Failure("Validation service unavailable"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Falha ao validar serviço");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceIsInvalid_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: false,
            InvalidServiceIds: new[] { serviceId },
            InactiveServiceIds: Array.Empty<Guid>());

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("não existe");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceIsInactive_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        var validationResult = new ModuleServiceValidationResultDto(
            AllValid: false,
            InvalidServiceIds: Array.Empty<Guid>(),
            InactiveServiceIds: new[] { serviceId });

        _serviceCatalogsMock
            .Setup(x => x.ValidateServicesAsync(It.IsAny<Guid[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ModuleServiceValidationResultDto>.Success(validationResult));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("não está ativo");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceDetailsRetrievalFails_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
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
            .ReturnsAsync(Result<ModuleServiceDto?>.Failure("Service details not found"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Falha ao recuperar detalhes do serviço");
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesThrows_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new AddServiceToProviderCommand(provider.Id.Value, serviceId);

        _providerRepositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
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
            .ReturnsAsync(Result<ModuleServiceDto?>.Success(new ModuleServiceDto(
                Id: serviceId,
                ProviderId: Guid.Empty,
                CategoryId: Guid.NewGuid(),
                CategoryName: "Category",
                Name: "Test Service",
                Description: "Description",
                IsActive: true)));

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Contain("Ocorreu um erro ao adicionar serviço ao prestador");
        result.Error.Message.Should().Contain("Database error");
    }
}



