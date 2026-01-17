using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Exceptions;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Contracts.Functional;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class SuspendProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<SuspendProviderCommandHandler>> _loggerMock;
    private readonly SuspendProviderCommandHandler _handler;

    public SuspendProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<SuspendProviderCommandHandler>>();

        _handler = new SuspendProviderCommandHandler(
            _providerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldSuspendProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        // Provider precisa estar Active para ser suspenso
        provider.CompleteBasicInfo();
        provider.Activate();

        var command = new SuspendProviderCommand(
            providerId,
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _providerRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue(
            $"Expected success but got error: {(result.IsFailure ? result.Error.Message : "unknown")}");
        provider.Status.Should().Be(EProviderStatus.Suspended);
        provider.SuspensionReason.Should().Be("Policy violation");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Provider not found");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            reason!);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Suspension reason is required");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptySuspendedBy_ShouldReturnFailure(string? suspendedBy)
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            suspendedBy!,
            "Policy violation");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("SuspendedBy is required");

        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to suspend provider");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDomainValidationFails_ShouldReturnDomainErrorMessage()
    {
        // Arrange
        var command = new SuspendProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Policy violation");

        // Create provider in PendingBasicInfo status (default after creation)
        // Attempting to suspend will fail with domain exception because only Active providers can be suspended
        var provider = ProviderBuilder.Create()
            .WithType(EProviderType.Individual)
            .Build();

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        // Domain exception message should be propagated
        result.Error.Message.Should().StartWith("Invalid status transition");

        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Provider>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
