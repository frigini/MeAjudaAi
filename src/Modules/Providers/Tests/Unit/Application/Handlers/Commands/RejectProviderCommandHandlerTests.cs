using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class RejectProviderCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<RejectProviderCommandHandler>> _loggerMock;
    private readonly RejectProviderCommandHandler _handler;

    public RejectProviderCommandHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<RejectProviderCommandHandler>>();
        
        _handler = new RejectProviderCommandHandler(
            _providerRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldRejectProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .WithVerificationStatus(EVerificationStatus.Pending)
            .Build();
        
        var command = new RejectProviderCommand(
            providerId,
            "admin@test.com",
            "Documents not valid");

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.Rejected);
        
        _providerRepositoryMock.Verify(
            r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProviderNotFound_ShouldReturnFailure()
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Documents not valid");

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
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            reason!);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Rejection reason is required");
        
        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task HandleAsync_WithEmptyRejectedBy_ShouldReturnFailure(string? rejectedBy)
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            rejectedBy!,
            "Documents not valid");

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("RejectedBy is required");
        
        _providerRepositoryMock.Verify(
            r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldReturnFailure()
    {
        // Arrange
        var command = new RejectProviderCommand(
            Guid.NewGuid(),
            "admin@test.com",
            "Documents not valid");

        _providerRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ProviderId>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Failed to reject provider");
    }
}
