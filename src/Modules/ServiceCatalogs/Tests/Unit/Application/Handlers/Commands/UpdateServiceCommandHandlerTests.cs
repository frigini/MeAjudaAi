using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Modules.ServiceCatalogs.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Moq;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class UpdateServiceCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _repositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly UpdateServiceCommandHandler _handler;

    public UpdateServiceCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        
        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>())
            .Returns(_repositoryMock.Object);
        
        _handler = new UpdateServiceCommandHandler(_uowMock.Object, _serviceQueriesMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Serviço Atualizado", "Descrição", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        service.Name.Should().Be("Serviço Atualizado");
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Serviço Existente", "Descrição", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync("Serviço Existente", service.Id, service.CategoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Já existe um serviço com o nome");
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        var command = new UpdateServiceCommand(Guid.NewGuid(), "Nome", " Desc", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Service?)null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyName_ShouldReturnFailure()

    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "", "Descrição", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("nome");
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionOccurs_ShouldReturnFailure()
    {
        var category = new ServiceCategoryBuilder().AsActive().Build();
        var service = new ServiceBuilder()
            .WithCategoryId(category.Id)
            .WithName("Serviço Original")
            .Build();
        var command = new UpdateServiceCommand(service.Id.Value, "Novo Nome", "Descrição", 1);

        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        _serviceQueriesMock.Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), It.IsAny<ServiceId>(), It.IsAny<ServiceCategoryId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
            
        // Simula erro de domínio no método Update que é chamado pelo handler
        _repositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ServiceId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Como o método Update está no agregado e não no handler, 
        // a única forma do handler capturar isso é se o Aggregate.Update lançar
        // Mas o UpdateServiceCommandHandler captura CatalogDomainException.
        // O teste precisa simular um erro que venha do domínio.
        // Vamos forçar um erro na UoW SaveChangesAsync.
        _uowMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions.CatalogDomainException("Erro de negócio"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
    }
}

