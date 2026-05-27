using FluentAssertions;
using MeAjudaAi.Modules.Providers.Application.Commands;
using MeAjudaAi.Modules.Providers.Application.Handlers.Commands;
using MeAjudaAi.Modules.Providers.Domain.Enums;

using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Application.Handlers.Commands;

public sealed class CompleteBasicInfoCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>> _mockRepository;
    private readonly Mock<ILogger<CompleteBasicInfoCommandHandler>> _mockLogger;
    private readonly CompleteBasicInfoCommandHandler _handler;

    public CompleteBasicInfoCommandHandlerTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>>();
        _mockLogger = new Mock<ILogger<CompleteBasicInfoCommandHandler>>();

        _uowMock.Setup(u => u.GetRepository<MeAjudaAi.Modules.Providers.Domain.Entities.Provider, MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>()).Returns(_mockRepository.Object);
        _handler = new CompleteBasicInfoCommandHandler(_uowMock.Object, _mockLogger.Object);
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

        _mockRepository.Setup(r => r.TryFindAsync(It.Is<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Status.Should().Be(EProviderStatus.PendingDocumentVerification);

        _mockRepository.Verify(r => r.TryFindAsync(It.Is<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ReturnsFailure()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var command = new CompleteBasicInfoCommand(providerId, "admin@test.com");

        _mockRepository.Setup(r => r.TryFindAsync(It.Is<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((MeAjudaAi.Modules.Providers.Domain.Entities.Provider?)null);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Provider not found");

        _mockRepository.Verify(r => r.TryFindAsync(It.Is<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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

        _mockRepository.Setup(r => r.TryFindAsync(It.Is<MeAjudaAi.Modules.Providers.Domain.ValueObjects.ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        _uowMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Failed to complete provider basic info");
    }
}
