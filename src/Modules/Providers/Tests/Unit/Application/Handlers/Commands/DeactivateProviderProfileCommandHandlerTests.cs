using FluentAssertions;
using MeAjudaAi.Contracts.Functional;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Application.Queries;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

[Trait("Category", "Unit")]
public class DeactivateProviderProfileCommandHandlerTests
{
    private readonly Mock<IProviderUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<DeactivateProviderProfileCommandHandler>> _loggerMock;
    private readonly DeactivateProviderProfileCommandHandler _sut;

    public DeactivateProviderProfileCommandHandlerTests()
    {
        _uowMock = new Mock<IProviderUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<DeactivateProviderProfileCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _sut = new DeactivateProviderProfileCommandHandler(_uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidProviderId_ShouldDeactivateAndReturnSuccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new DeactivateProviderProfileCommand(providerId);
        var provider = new ProviderBuilder().WithId(providerId).Build();
        // IsActive is true by default
        provider.ClearDomainEvents();

        _providerRepositoryMock.Setup(repo => repo.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(provider);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.IsActive.Should().BeFalse();
        _uowMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeactivateProviderProfileCommand(Guid.NewGuid());

        _providerRepositoryMock.Setup(repo => repo.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync((Provider?)null);

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
        _uowMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeactivateProviderProfileCommand(Guid.NewGuid());
        var provider = new ProviderBuilder().WithId(command.ProviderId).Build();

        _providerRepositoryMock.Setup(repo => repo.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
                               .ReturnsAsync(provider);
        _uowMock.Setup(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()))
                               .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        _uowMock.Verify(repo => repo.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}


