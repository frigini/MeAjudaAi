using MeAjudaAi.Modules.Providers.Domain.Entities;
using MeAjudaAi.Modules.Providers.Domain.Repositories;
using MeAjudaAi.Modules.Providers.Domain.ValueObjects;
using MeAjudaAi.Modules.Providers.Infrastructure.Events.Handlers.Integration;
using MeAjudaAi.Modules.Providers.Tests.Builders;
using MeAjudaAi.Shared.Messaging.Messages.Documents;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Providers.Tests.Unit.Infrastructure.Events;

public class DocumentVerifiedIntegrationEventHandlerTests
{
    private readonly Mock<IProviderRepository> _providerRepositoryMock;
    private readonly Mock<ILogger<DocumentVerifiedIntegrationEventHandler>> _loggerMock;
    private readonly DocumentVerifiedIntegrationEventHandler _handler;

    public DocumentVerifiedIntegrationEventHandlerTests()
    {
        _providerRepositoryMock = new Mock<IProviderRepository>();
        _loggerMock = new Mock<ILogger<DocumentVerifiedIntegrationEventHandler>>();
        _handler = new DocumentVerifiedIntegrationEventHandler(
            _providerRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task HandleAsync_WithValidProvider_ShouldLogDocumentVerification()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents",
            documentId,
            providerId,
            "CNH",
            true,
            DateTime.UtcNow
        );

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Verify logging occurred
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Document") && v.ToString()!.Contains("verified")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentProvider_ShouldLogWarning()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents",
            documentId,
            providerId,
            "CNH",
            true,
            DateTime.UtcNow
        );

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Provider?)null);

        // Act
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()),
            Times.Once
        );

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Provider") && v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task HandleAsync_WithDifferentDocumentTypes_ShouldProcessAll()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentTypes = new[] { "CNH", "RG", "CPF", "CRM" };

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        foreach (var documentType in documentTypes)
        {
            var integrationEvent = new DocumentVerifiedIntegrationEvent(
                "Documents",
                Guid.NewGuid(),
                providerId,
                documentType,
                false,
                DateTime.UtcNow
            );

            // Act
            await _handler.HandleAsync(integrationEvent, CancellationToken.None);

            // Assert - should log for each document type
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(documentType)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.AtLeastOnce
            );
        }
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents",
            Guid.NewGuid(),
            providerId,
            "CNH",
            true,
            DateTime.UtcNow
        );

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.HandleAsync(integrationEvent, CancellationToken.None)
        );

        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Theory]
    [InlineData("CNH", true)]
    [InlineData("RG", false)]
    [InlineData("CPF", true)]
    [InlineData("CRM", false)]
    public async Task HandleAsync_WithVariousDocumentConfigurations_ShouldProcess(string documentType, bool hasOcrData)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var integrationEvent = new DocumentVerifiedIntegrationEvent(
            "Documents",
            Guid.NewGuid(),
            providerId,
            documentType,
            hasOcrData,
            DateTime.UtcNow
        );

        var provider = new ProviderBuilder()
            .WithId(providerId)
            .Build();

        _providerRepositoryMock
            .Setup(x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(provider);

        // Act
        await _handler.HandleAsync(integrationEvent, CancellationToken.None);

        // Assert
        _providerRepositoryMock.Verify(
            x => x.GetByIdAsync(It.Is<ProviderId>(p => p.Value == providerId), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
