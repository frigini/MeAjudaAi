using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Enums;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using Microsoft.Extensions.Logging;
using Moq;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class CompleteBasicInfoCommandHandlerTests
{
    private readonly Mock<IProviderRepository> _mockRepository;
    private readonly Mock<ILogger<CompleteBasicInfoCommandHandler>> _mockLogger;
    private readonly CompleteBasicInfoCommandHandler _handler;

    public CompleteBasicInfoCommandHandlerTests()
    {
        _mockRepository = new Mock<IProviderRepository>();
        _mockLogger = new Mock<ILogger<CompleteBasicInfoCommandHandler>>();
        _handler = new CompleteBasicInfoCommandHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidProvider_CompletesBasicInfo()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        var command = new CompleteBasicInfoCommand(providerId, "admin@test.com");

        _mockRepository.Setup(r => r.GetByIdAsync(It.Is<Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockRepository.Setup(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.PendingDocumentVerification);

        _mockRepository.Verify(r => r.GetByIdAsync(It.Is<Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CompleteBasicInfoCommand(providerId, "admin@test.com");

        _mockRepository.Setup(r => r.GetByIdAsync(It.Is<Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Provider not found");

        _mockRepository.Verify(r => r.GetByIdAsync(It.Is<Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithRepositoryException_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        var command = new CompleteBasicInfoCommand(providerId, "admin@test.com");

        _mockRepository.Setup(r => r.GetByIdAsync(It.Is<Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _mockRepository.Setup(r => r.UpdateAsync(provider, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Failed to complete provider basic info");
    }
}
