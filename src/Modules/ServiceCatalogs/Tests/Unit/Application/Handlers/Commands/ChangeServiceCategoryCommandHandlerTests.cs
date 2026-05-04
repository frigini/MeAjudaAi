using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ChangeServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _serviceRepoMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _categoryRepoMock;
    private readonly ChangeServiceCategoryCommandHandler _handler;

    public ChangeServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepoMock = new Mock<IRepository<Service, ServiceId>>();
        _categoryRepoMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>())
            .Returns(_serviceRepoMock.Object);
        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>())
            .Returns(_categoryRepoMock.Object);
        
        _handler = new ChangeServiceCategoryCommandHandler(_uowMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var oldCategory = new ServiceCategoryBuilder().WithName("Limpeza").AsActive().Build();
        var newCategory = new ServiceCategoryBuilder().WithName("Manutenção").AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(oldCategory.Id)
            .WithName("Serviço 1")
            .Build();
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _serviceRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _categoryRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        var command = new ChangeServiceCategoryCommand(Guid.NewGuid(), Guid.NewGuid());

        _serviceRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyServiceId_ShouldReturnFailure()
    {
        var command = new ChangeServiceCategoryCommand(Guid.Empty, Guid.NewGuid());

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}