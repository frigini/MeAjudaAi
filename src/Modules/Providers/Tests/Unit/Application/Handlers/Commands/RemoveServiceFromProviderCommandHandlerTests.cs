using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Component", "CommandHandler")]
public class RemoveServiceFromProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _repositoryMock;
    private readonly Mock<ILogger<RemoveServiceFromProviderCommandHandler>> _loggerMock;
    private readonly RemoveServiceFromProviderCommandHandler _sut;

    public RemoveServiceFromProviderCommandHandlerTests()
    {
        _repositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<RemoveServiceFromProviderCommandHandler>>();

        _sut = new RemoveServiceFromProviderCommandHandler(
            _repositoryMock.Object,
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
            .Setup(x => x.GetByIdAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Services.Should().NotContain(s => s.ServiceId == serviceId);
        _repositoryMock.Verify(x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ShouldReturnFailure()
    {
        // Arrange
        var command = new RemoveServiceFromProviderCommand(Guid.NewGuid(), Guid.NewGuid());

        _repositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Prestador não encontrado");
        _repositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingNonExistentService_ShouldReturnFailure()
    {
        // Arrange
        var serviceId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().Build();
        var command = new RemoveServiceFromProviderCommand(provider.Id.Value, serviceId);

        _repositoryMock
            .Setup(x => x.GetByIdAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao remover serviço do prestador");
        _repositoryMock.Verify(x => x.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(x => x.GetByIdAsync(new ProviderId(provider.Id.Value), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _repositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Ocorreu um erro ao remover serviço do prestador");
    }
}
