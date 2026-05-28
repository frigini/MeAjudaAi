using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs;
using MeAjudaAi.Contracts.Modules.ServiceCatalogs.DTOs;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class AddServiceToProviderCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<IServiceCatalogsModuleApi> _serviceCatalogsMock;
    private readonly Mock<ILogger<AddServiceToProviderCommandHandler>> _loggerMock;
    private readonly AddServiceToProviderCommandHandler _handler;

    public AddServiceToProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
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
}
