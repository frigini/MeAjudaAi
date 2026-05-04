using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using Moq;
using Xunit;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceEntity, ServiceId>> _serviceRepoMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _serviceRepoMock = new Mock<IRepository<ServiceEntity, ServiceId>>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceEntity, ServiceId>())
            .Returns(_serviceRepoMock.Object);
        
        _handler = new DeleteServiceCommandHandler(_uowMock.Object, _providersModuleApiMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenServiceExistsAndNotUsed_ShouldDeleteAndReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _serviceRepoMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _providersModuleApiMock.Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WhenIdIsEmpty_ShouldReturnFailure()
    {
        var command = new DeleteServiceCommand(Guid.Empty);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}