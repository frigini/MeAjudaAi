using FluentAssertions;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Moq;
using Xunit;
using ServiceEntity = MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities.Service;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Application.Handlers.Commands.Service;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceEntity, ServiceId>> _repositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<ServiceEntity, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        
        _uowMock.Setup(x => x.GetRepository<ServiceEntity, ServiceId>())
            .Returns(_repositoryMock.Object);
        
        _handler = new UpdateServiceCommandHandler(_uowMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenIdIsEmpty_ShouldReturnFailure()
    {
        var command = new UpdateServiceCommand(Guid.Empty, "Name", "Desc", 1);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Atualizado", "Desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Existente", "Desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync("Existente", service.Id, service.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("existe nesta categoria");
    }
}