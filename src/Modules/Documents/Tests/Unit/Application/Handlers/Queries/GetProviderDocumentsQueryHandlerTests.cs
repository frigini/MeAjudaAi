using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers.Queries;

public class GetProviderDocumentsQueryHandlerTests
{
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly GetProviderDocumentsQueryHandler _handler;

    public GetProviderDocumentsQueryHandlerTests()
    {
        _mockQueries = new Mock<IDocumentQueries>();
        _handler = new GetProviderDocumentsQueryHandler(_mockQueries.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocuments_ShouldReturnDocumentList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var documents = new List<Document>
        {
            Document.Create(providerId, EDocumentType.IdentityDocument, "doc1.pdf", "url1"),
            Document.Create(providerId, EDocumentType.ProofOfResidence, "doc2.pdf", "url2")
        };

        _mockQueries.Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(documents);

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(d => d.Id == documents[0].Id);
        result.Should().Contain(d => d.Id == documents[1].Id);

        _mockQueries.Verify(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNoDocuments_ShouldReturnEmptyList()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        _mockQueries
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>());

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.CriminalRecord, "crime.pdf", "storage/crime.pdf");
        document.MarkAsPendingVerification();

        _mockQueries
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { document });

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = (await _handler.HandleAsync(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        var dto = result[0];
        dto.Id.Should().Be(document.Id);
        dto.ProviderId.Should().Be(providerId);
        dto.DocumentType.Should().Be(EDocumentType.CriminalRecord);
        dto.FileName.Should().Be("crime.pdf");
        dto.FileUrl.Should().Be("storage/crime.pdf");
        dto.Status.Should().Be(EDocumentStatus.PendingVerification);
        dto.UploadedAt.Should().BeCloseTo(document.UploadedAt, TimeSpan.FromSeconds(1));
        dto.VerifiedAt.Should().BeNull();
        dto.RejectionReason.Should().BeNull();
        dto.OcrData.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentIsVerified_ShouldMapVerifiedFields()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "id.pdf", "storage/id.pdf");
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"name\":\"João\"}");

        _mockQueries
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { document });

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = (await _handler.HandleAsync(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(EDocumentStatus.Verified);
        result[0].VerifiedAt.Should().NotBeNull();
        result[0].OcrData.Should().Be("{\"name\":\"João\"}");
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentIsRejected_ShouldMapRejectionReason()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.ProofOfResidence, "proof.pdf", "storage/proof.pdf");
        document.MarkAsPendingVerification();
        document.MarkAsRejected("Document is blurry");

        _mockQueries
            .Setup(x => x.GetByProviderIdAsync(providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { document });

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        var result = (await _handler.HandleAsync(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(EDocumentStatus.Rejected);
        result[0].RejectionReason.Should().Be("Document is blurry");
    }

    [Fact]
    public async Task HandleAsync_ShouldPassCancellationTokenToQuery()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();

        _mockQueries
            .Setup(x => x.GetByProviderIdAsync(providerId, cts.Token))
            .ReturnsAsync(new List<Document>());

        var query = new GetProviderDocumentsQuery(providerId);

        // Act
        await _handler.HandleAsync(query, cts.Token);

        // Assert
        _mockQueries.Verify(
            x => x.GetByProviderIdAsync(providerId, cts.Token),
            Times.Once);
    }
}
