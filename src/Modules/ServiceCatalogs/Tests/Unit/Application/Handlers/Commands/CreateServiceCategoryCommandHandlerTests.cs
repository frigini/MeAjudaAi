using MeAjudaAi.Modules.ServiceCatalogs.Application.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Handlers.Commands.ServiceCategory;
using MeAjudaAi.Modules.ServiceCatalogs.Application.Queries.Interfaces;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Entities;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.Exceptions;
using MeAjudaAi.Modules.ServiceCatalogs.Domain.ValueObjects;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Localization;

namespace MeAjudaAi.Modules.ServiceCatalogs.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "ServiceCatalogs")]
[Trait("Layer", "Application")]
public class CreateServiceCategoryCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<ServiceCategory, ServiceCategoryId>> _repositoryMock;
    private readonly Mock<IServiceCategoryQueries> _queriesMock;
    private readonly Mock<IStringLocalizer<Strings>> _localizerMock;
    private readonly CreateServiceCategoryCommandHandler _handler;

    public CreateServiceCategoryCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<ServiceCategory, ServiceCategoryId>>();
        _queriesMock = new Mock<IServiceCategoryQueries>();
        _localizerMock = new Mock<IStringLocalizer<Strings>>();
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "CategoryNameRequired")]).Returns(new LocalizedString("CategoryNameRequired", "O nome da categoria é obrigatório."));
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "CategoryNameAlreadyExists"), It.IsAny<object[]>()]).Returns((string key, object[] args) => new LocalizedString(key, $"Já existe uma categoria com o nome '{args[0]}'."));
        _localizerMock.Setup(x => x[It.Is<string>(s => s == "CategoryCreateError")]).Returns(new LocalizedString("CategoryCreateError", "Ocorreu um erro inesperado ao processar a solicitação."));

        _uowMock.Setup(x => x.GetRepository<ServiceCategory, ServiceCategoryId>()).Returns(_repositoryMock.Object);
        _handler = new CreateServiceCategoryCommandHandler(_uowMock.Object, _queriesMock.Object, NullLogger<CreateServiceCategoryCommandHandler>.Instance, _localizerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccess()
    {
        // Arrange
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(Guid.Empty);

        _queriesMock.Verify(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.Add(It.Is<ServiceCategory>(sc =>
            sc.Name == command.Name &&
            sc.Description == command.Description &&
            sc.DisplayOrder == command.DisplayOrder &&
            sc.IsActive == true)), Times.Once);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateServiceCategoryCommand("Limpeza", "Serviços de limpeza", 1);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("Já existe uma categoria com o nome");

        _queriesMock.Verify(x => x.ExistsWithNameAsync(command.Name, null, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.Add(It.IsAny<ServiceCategory>()), Times.Never);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_WithInvalidName_ShouldReturnFailure(string? invalidName)
    {
        // Arrange
        var command = new CreateServiceCategoryCommand(invalidName!, "Description", 1);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("O nome da categoria é obrigatório.", "validation error should mention the problematic field");
    }

    [Fact]
    public async Task Handle_WhenDomainExceptionThrown_ShouldReturnFailureWithMessage()
    {
        // Arrange
        var command = new CreateServiceCategoryCommand("Valid Name", "Description", 1);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new CatalogDomainException("Domain rule violation"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Domain rule violation");
    }

    [Fact]
    public async Task Handle_WhenGenericExceptionThrown_ShouldReturnGenericFailure()
    {
        var command = new CreateServiceCategoryCommand("Valid Name", "Description", 1);

        _queriesMock
            .Setup(x => x.ExistsWithNameAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected DB error"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Be("Ocorreu um erro inesperado ao processar a solicitação.");
    }
}



