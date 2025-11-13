using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers;

public class GetDocumentStatusQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<ILogger<GetDocumentStatusQueryHandler>> _loggerMock;
    private readonly GetDocumentStatusQueryHandler _handler;

    public GetDocumentStatusQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _loggerMock = new Mock<ILogger<GetDocumentStatusQueryHandler>>();
        _handler = new GetDocumentStatusQueryHandler(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnDocumentDto_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = new Document(
            providerId,
            DocumentType.IdentityDocument,
            "https://storage.example.com/file.pdf",
            "file.pdf");

        typeof(Document)
            .GetProperty("Id")!
            .SetValue(document, documentId);

        document.MarkAsVerified(new { Name = "Test" });

        var query = new GetDocumentStatusQuery
        {
            DocumentId = documentId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(documentId);
        result.ProviderId.Should().Be(providerId);
        result.DocumentType.Should().Be(DocumentType.IdentityDocument);
        result.Status.Should().Be(DocumentStatus.Verified);
        result.FileName.Should().Be("file.pdf");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var query = new GetDocumentStatusQuery
        {
            DocumentId = documentId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow.AddHours(-1);
        var verifiedAt = DateTime.UtcNow;
        
        var document = new Document(
            providerId,
            DocumentType.CriminalRecord,
            "https://storage.example.com/criminal-record.pdf",
            "criminal-record.pdf");

        typeof(Document).GetProperty("Id")!.SetValue(document, documentId);
        typeof(Document).GetProperty("UploadedAt")!.SetValue(document, uploadedAt);

        var ocrData = new { RecordNumber = "ABC123" };
        document.MarkAsVerified(ocrData);

        var query = new GetDocumentStatusQuery
        {
            DocumentId = documentId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.UploadedAt.Should().Be(uploadedAt);
        result.VerifiedAt.Should().NotBeNull();
        result.RejectionReason.Should().BeNull();
    }
}
