using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.Service;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.ServiceCatalogs;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class ChangeServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Service, ServiceId>> _repositoryMock;
    private readonly Mock<IServiceQueries> _serviceQueriesMock;
    private readonly Mock<IServiceCategoryQueries> _categoryQueriesMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly ChangeServiceCategoryCommandHandler _handler;

    public ChangeServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Service, ServiceId>>();
        _serviceQueriesMock = new Mock<IServiceQueries>();
        _categoryQueriesMock = new Mock<IServiceCategoryQueries>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();

        _uowMock.Setup(x => x.GetRepository<Service, ServiceId>()).Returns(_repositoryMock.Object);

        _handler = new ChangeServiceCategoryCommandHandler(
            _uowMock.Object,
            _serviceQueriesMock.Object,
            _categoryQueriesMock.Object,
            NullLogger<ChangeServiceCategoryCommandHandler>.Instance,
            _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var oldCategory = new ServiceCategoryBuilder().AsActive().WithName("Original").Build();
        var newCategory = new ServiceCategoryBuilder().AsActive().WithName("New").Build();
        var service = Service.Create(oldCategory.Id, "Service", "Desc", 1);

        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, service.Id, newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        service.CategoryId.Should().Be(newCategory.Id);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyServiceId_ShouldThrow()
    {
        // Arrange
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "ServiceIdRequired")]).Returns(new LocalizedString("ServiceIdRequired", "O ID do serviço é obrigatório."));

        var command = new ChangeServiceCategoryCommand(Guid.Empty, Guid.NewGuid());

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.UnprocessableEntityException>()
            .WithMessage("*serviço*obrigatório*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyCategoryId_ShouldThrow()
    {
        // Arrange
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "NewCategoryIdRequired")]).Returns(new LocalizedString("NewCategoryIdRequired", "O ID da nova categoria é obrigatório."));

        var command = new ChangeServiceCategoryCommand(Guid.NewGuid(), Guid.Empty);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.UnprocessableEntityException>()
            .WithMessage("*categoria*obrigatório*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var command = new ChangeServiceCategoryCommand(Guid.NewGuid(), Guid.NewGuid());

        _localizerMock.Setup(x => x[It.Is<string>(s => s == "ServiceNotFoundById"), It.IsAny<object[]>()]).Returns(new LocalizedString("ServiceNotFoundById", $"Serviço com ID '{command.ServiceId}' não encontrado."));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("não encontrado.");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentCategory_ShouldThrow()
    {
        // Arrange
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Service", "Desc", 1);
        var command = new ChangeServiceCategoryCommand(service.Id.Value, Guid.NewGuid());

        _localizerMock.Setup(x => x[It.Is<string>(s => s == "CategoryNotFoundById"), It.IsAny<object[]>()]).Returns(new LocalizedString("CategoryNotFoundById", $"Categoria com ID '{command.NewCategoryId}' não encontrada."));

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.UnprocessableEntityException>()
            .WithMessage("*Categoria*não encontrada*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveCategory_ShouldThrow()
    {
        // Arrange
        var inactiveCategory = new ServiceCategoryBuilder().AsInactive().Build();
        var service = Service.Create(inactiveCategory.Id, "Service", "Desc", 1);
        var command = new ChangeServiceCategoryCommand(service.Id.Value, inactiveCategory.Id.Value);

        _localizerMock.Setup(x => x[It.Is<string>(s => s == "CannotMoveToInactiveCategory")]).Returns(new LocalizedString("CannotMoveToInactiveCategory", "Não é possível mover o serviço para uma categoria inativa."));

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(inactiveCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveCategory);

        // Act
        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.UnprocessableEntityException>()
            .WithMessage("*categoria inativa*");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithDuplicateNameInTargetCategory_ShouldReturnFailure()
    {
        // Arrange
        var newCategory = new ServiceCategoryBuilder().AsActive().Build();
        var service = Service.Create(ServiceCategoryId.From(Guid.NewGuid()), "Service", "Desc", 1);
        var command = new ChangeServiceCategoryCommand(service.Id.Value, newCategory.Id.Value);

        _localizerMock.Setup(x => x[It.Is<string>(s => s == "ServiceNameExistsInCategory"), It.IsAny<object[]>()]).Returns(new LocalizedString("ServiceNameExistsInCategory", $"Já existe um serviço com o nome '{service.Name}' na categoria de destino."));

        _repositoryMock
            .Setup(x => x.TryFindAsync(service.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(service);
        _categoryQueriesMock
            .Setup(x => x.GetByIdAsync(newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCategory);
        _serviceQueriesMock
            .Setup(x => x.ExistsWithNameAsync(service.Name, service.Id, newCategory.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Já existe um serviço com o nome");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
