using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

/// <summary>
/// Testes unitários para o handler DeleteMyProviderProfileCommandHandler.
/// </summary>
public class DeleteMyProviderProfileCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<ILogger<DeleteMyProviderProfileCommandHandler>> _loggerMock;
    private readonly DeleteMyProviderProfileCommandHandler _handler;

    public DeleteMyProviderProfileCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _timeProviderMock = new Mock<TimeProvider>();
        _loggerMock = new Mock<ILogger<DeleteMyProviderProfileCommandHandler>>();

        _handler = new DeleteMyProviderProfileCommandHandler(
            _providerRepositoryMock.Object,
            _timeProviderMock.Object,
            _loggerMock.Object);
    }

    /// <summary>
    /// Testa exclusão de perfil quando provider existe deve fazer soft delete.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenProviderExists_ShouldSoftDeleteProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeleteMyProviderProfileCommand(providerId);
        
        var provider = new ProviderBuilder().WithId(providerId).Build();

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.IsDeleted.Should().BeTrue();
        
        _providerRepositoryMock.Verify(x => x.UpdateAsync(It.Is<Provider>(p => p.Id.Value == providerId && p.IsDeleted), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Testa exclusão de perfil quando provider não existe deve retornar erro.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WhenProviderDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeleteMyProviderProfileCommand(providerId);

        _providerRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.Message.Should().Contain("not found");
        
        _providerRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
