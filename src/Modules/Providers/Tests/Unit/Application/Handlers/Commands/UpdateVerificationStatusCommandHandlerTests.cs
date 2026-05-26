using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;

using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class UpdateVerificationStatusCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<Provider, ProviderId>> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateVerificationStatusCommandHandler>> _loggerMock;
    private readonly UpdateVerificationStatusCommandHandler _handler;

    public UpdateVerificationStatusCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _providerRepositoryMock = new Mock<IRepository<Provider, ProviderId>>();
        _loggerMock = new Mock<ILogger<UpdateVerificationStatusCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<Provider, ProviderId>()).Returns(_providerRepositoryMock.Object);
        _handler = new UpdateVerificationStatusCommandHandler(_uowMock.Object, _loggerMock.Object);
    }

    [Theory]
    [InlineData(EVerificationStatus.Pending)]
    [InlineData(EVerificationStatus.InProgress)]
    [InlineData(EVerificationStatus.Verified)]
    [InlineData(EVerificationStatus.Rejected)]
    public async Task HandleAsync_WithValidStatus_ShouldReturnSuccessResult(EVerificationStatus status)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create().WithId(providerId);

        var command = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: status
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.Verified
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.Verified
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("An error occurred while updating the verification status");

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_StatusTransitionFromPendingToVerified_ShouldWork()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithVerificationStatus(EVerificationStatus.Pending);

        var command = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.Verified
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_StatusTransitionFromVerifiedToRejected_ShouldWork()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithVerificationStatus(EVerificationStatus.Verified);

        var command = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.Rejected
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_MultipleStatusUpdates_ShouldWorkCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = ProviderBuilder.Create()
            .WithId(providerId)
            .WithVerificationStatus(EVerificationStatus.Pending);

        // Primeiro comando: Pending -> InProgress
        var command1 = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.InProgress
        );

        // Segundo comando: InProgress -> Verified
        var command2 = new UpdateVerificationStatusCommand(
            ProviderId: providerId,
            Status: EVerificationStatus.Verified
        );

        _providerRepositoryMock
            .Setup(r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act & Assert - Primeira atualização
        var result1 = await _handler.HandleAsync(command1, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();

        // Act & Assert - Segunda atualização
        var result2 = await _handler.HandleAsync(command2, CancellationToken.None);
        result2.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.TryFindAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _uowMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
