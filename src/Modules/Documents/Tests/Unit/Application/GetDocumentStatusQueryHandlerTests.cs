using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class GetDocumentStatusQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<ILogger<GetDocumentStatusQueryHandler>> _mockLogger;
    private readonly GetDocumentStatusQueryHandler _handler;

    public GetDocumentStatusQueryHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockLogger = new Mock<ILogger<GetDocumentStatusQueryHandler>>();
        _handler = new GetDocumentStatusQueryHandler(_mockRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldReturnDocumentDto()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var query = new GetDocumentStatusQuery(documentId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
        result.ProviderId.Should().Be(document.ProviderId);
        result.DocumentType.Should().Be(document.DocumentType);
        result.FileName.Should().Be(document.FileName);
        result.Status.Should().Be(document.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var query = new GetDocumentStatusQuery(documentId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllDocumentProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var query = new GetDocumentStatusQuery(documentId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
        result.ProviderId.Should().Be(document.ProviderId);
        result.DocumentType.Should().Be(document.DocumentType);
        result.FileName.Should().Be(document.FileName);
        result.FileUrl.Should().Be(document.FileUrl);
        result.Status.Should().Be(document.Status);
        result.UploadedAt.Should().Be(document.UploadedAt);
        result.VerifiedAt.Should().Be(document.VerifiedAt);
        result.RejectionReason.Should().Be(document.RejectionReason);
        result.OcrData.Should().Be(document.OcrData);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldPropagateException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var query = new GetDocumentStatusQuery(documentId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(query, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldLogWarning()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var query = new GetDocumentStatusQuery(documentId);

        // Act
        await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(documentId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
