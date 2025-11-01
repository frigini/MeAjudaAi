using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Commands;

[Trait("Category", "Unit")]
[Trait("Module", "Providers")]
[Trait("Layer", "Application")]
public class UpdateVerificationStatusCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<UpdateVerificationStatusCommandHandler>> _loggerMock;
    private readonly UpdateVerificationStatusCommandHandler _handler;

    public UpdateVerificationStatusCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<UpdateVerificationStatusCommandHandler>>();
        _handler = new UpdateVerificationStatusCommandHandler(_providerRepositoryMock.Object, _loggerMock.Object);
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Error updating verification status");
        result.Error.Message.Should().Contain("Database error");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(providerId);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
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
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert - Primeira atualização
        var result1 = await _handler.HandleAsync(command1, CancellationToken.None);
        result1.IsSuccess.Should().BeTrue();

        // Act & Assert - Segunda atualização
        var result2 = await _handler.HandleAsync(command2, CancellationToken.None);
        result2.IsSuccess.Should().BeTrue();

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
