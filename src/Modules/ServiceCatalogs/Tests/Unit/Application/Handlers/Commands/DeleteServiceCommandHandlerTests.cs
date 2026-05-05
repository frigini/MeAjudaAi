using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Contracts.Modules.Providers;
using MeAjudaAi.Contracts.Functional;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class DeleteServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _repositoryMock;
    private readonly Mock<IProvidersModuleApi> _providersModuleApiMock;
    private readonly DeleteServiceCommandHandler _handler;

    public DeleteServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _providersModuleApiMock = new Mock<IProvidersModuleApi>();
        
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>())
            .Returns(_repositoryMock.Object);
        
        _handler = new DeleteServiceCommandHandler(_uowMock.Object, _providersModuleApiMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndNoProviders_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _providersModuleApiMock.Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(false));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        var command = new DeleteServiceCommand(Guid.NewGuid());

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenProvidersApiFails_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _providersModuleApiMock.Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Failure("API Error"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("API Error");
    }

    [Fact]
    public async Task Handle_WhenServiceIsOfferedByProviders_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Limpeza")
            .Build();
        var command = new DeleteServiceCommand(service.Id.Value);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _providersModuleApiMock.Setup(x => x.HasProvidersOfferingServiceAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<bool>.Success(true));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("pois ele é oferecido por prestadores");
    }
}

