using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Aggregates;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers;

public class GetProviderDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<ILogger<GetProviderDocumentsQueryHandler>> _loggerMock;
    private readonly GetProviderDocumentsQueryHandler _handler;

    public GetProviderDocumentsQueryHandlerTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _loggerMock = new Mock<ILogger<GetProviderDocumentsQueryHandler>>();
        _handler = new GetProviderDocumentsQueryHandler(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnAllDocuments_ForProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            new(providerId, DocumentType.IdentityDocument, "url1", "file1.pdf"),
            new(providerId, DocumentType.ProofOfResidence, "url2", "file2.pdf"),
            new(providerId, DocumentType.CriminalRecord, "url3", "file3.pdf")
        };

        var query = new GetProviderDocumentsQuery
        {
            ProviderId = providerId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(d => d.DocumentType == DocumentType.IdentityDocument);
        result.Should().Contain(d => d.DocumentType == DocumentType.ProofOfResidence);
        result.Should().Contain(d => d.DocumentType == DocumentType.CriminalRecord);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnEmptyList_WhenNoDocuments()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var query = new GetProviderDocumentsQuery
        {
            ProviderId = providerId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>());

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapDocumentsCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documentId = Guid.NewGuid();
        var uploadedAt = DateTime.UtcNow.AddDays(-1);
        
        var document = new Document(
            providerId,
            DocumentType.IdentityDocument,
            "https://storage.example.com/id.pdf",
            "identity-document.pdf");

        typeof(Document).GetProperty("Id")!.SetValue(document, documentId);
        typeof(Document).GetProperty("UploadedAt")!.SetValue(document, uploadedAt);
        document.MarkAsVerified(new { Name = "Test User" });

        var query = new GetProviderDocumentsQuery
        {
            ProviderId = providerId,
            CorrelationId = Guid.NewGuid().ToString()
        };

        _repositoryMock
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { document });

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        var dto = result.First();
        dto.Id.Should().Be(documentId);
        dto.ProviderId.Should().Be(providerId);
        dto.DocumentType.Should().Be(DocumentType.IdentityDocument);
        dto.Status.Should().Be(DocumentStatus.Verified);
        dto.FileName.Should().Be("identity-document.pdf");
        dto.UploadedAt.Should().Be(uploadedAt);
        dto.VerifiedAt.Should().NotBeNull();
    }
}
