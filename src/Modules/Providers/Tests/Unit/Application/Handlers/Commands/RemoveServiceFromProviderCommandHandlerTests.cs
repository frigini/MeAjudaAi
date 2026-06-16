using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "CommandHandler")]
public class RemoveServiceFromProviderCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _repositoryMock;
    private readonly Mock<ILogger<RemoveServiceFromProviderCommandHandler>> _loggerMock;
    private readonly RemoveServiceFromProviderCommandHandler _sut;

    public RemoveServiceFromProviderCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<RemoveServiceFromProviderCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_repositoryMock.Object);
        _sut = new RemoveServiceFromProviderCommandHandler(
            _uowMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidService_ShouldRemoveServiceFromProvider()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        provider.AddService(serviceId, "Service Name");
        var command = new RemoveServiceFromProviderCommand(provider.Id.Value, serviceId);

        _repositoryMock
            .Setup(x => x.TryFindAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Services.Should().NotContain(s => s.ServiceId == serviceId);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveServiceFromProviderCommand(Guid.NewGuid(), Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Prestador não encontrado");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new RemoveServiceFromProviderCommand(provider.Id.Value, serviceId);

        _repositoryMock
            .Setup(x => x.TryFindAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao remover serviço do prestador");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        provider.AddService(serviceId, "Service Name");
        var command = new RemoveServiceFromProviderCommand(provider.Id.Value, serviceId);

        _repositoryMock
            .Setup(x => x.TryFindAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _uowMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao remover serviço do prestador");
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}



