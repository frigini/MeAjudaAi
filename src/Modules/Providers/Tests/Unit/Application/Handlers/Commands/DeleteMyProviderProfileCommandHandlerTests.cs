using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

/// <summary>
/// Testes unitários para o handler DeleteMyProviderProfileCommandHandler.
/// </summary>
public class DeleteMyProviderProfileCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<TimeProvider> _timeProviderMock;
    private readonly Mock<ILogger<DeleteMyProviderProfileCommandHandler>> _loggerMock;
    private readonly DeleteMyProviderProfileCommandHandler _handler;

    public DeleteMyProviderProfileCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _timeProviderMock = new Mock<TimeProvider>();
        _loggerMock = new Mock<ILogger<DeleteMyProviderProfileCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new DeleteMyProviderProfileCommandHandler(
            _uowMock.Object,
            _timeProviderMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderExists_ShouldSoftDeleteProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeleteMyProviderProfileCommand(providerId);
        
        var provider = new ProviderBuilder().WithId(providerId).Build();

        _providerRepositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.IsDeleted.Should().BeTrue();
        
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderDoesNotExist_ShouldReturnNotFoundError()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeleteMyProviderProfileCommand(providerId);

        _providerRepositoryMock.Setup(x => x.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
        
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}


