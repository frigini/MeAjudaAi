using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Builders.Modules.Documents;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class GetDocumentByIdQueryHandlerTests
{
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<ILogger<GetDocumentByIdQueryHandler>> _mockLogger;
    private readonly GetDocumentByIdQueryHandler _handler;

    public GetDocumentByIdQueryHandlerTests()
    {
        _mockQueries = new Mock<IDocumentQueries>();
        _mockLogger = new Mock<ILogger<GetDocumentByIdQueryHandler>>();
        _handler = new GetDocumentByIdQueryHandler(_mockQueries.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldReturnDocumentDto()
    {
        // Arrange
        var document = new DocumentBuilder().AsIdentityDocument().WithFileName("test.pdf").WithFileUrl("blob-url").Build();
        var documentId = document.Id;

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var query = new GetDocumentByIdQuery(documentId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldReturnNull()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockQueries
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var query = new GetDocumentByIdQuery(documentId);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllFieldsCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = new DocumentBuilder().WithProviderId(providerId).AsCriminalRecord().WithFileName("crime.pdf").WithFileUrl("storage/crime.pdf").Build();
        document.MarkAsPendingVerification();

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var query = new GetDocumentByIdQuery(document.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(document.Id);
        result.ProviderId.Should().Be(providerId);
        result.DocumentType.Should().Be(EDocumentType.CriminalRecord);
        result.FileName.Should().Be("crime.pdf");
        result.FileUrl.Should().Be("storage/crime.pdf");
        result.Status.Should().Be(EDocumentStatus.PendingVerification);
        result.UploadedAt.Should().BeCloseTo(document.UploadedAt, TimeSpan.FromSeconds(1));
        result.VerifiedAt.Should().BeNull();
        result.RejectionReason.Should().BeNull();
        result.OcrData.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentIsVerified_ShouldMapVerifiedAtAndOcrData()
    {
        // Arrange
        var document = new DocumentBuilder().AsIdentityDocument().WithFileName("id.pdf").WithFileUrl("storage/id.pdf").Build();
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"name\":\"João\"}");

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var query = new GetDocumentByIdQuery(document.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(EDocumentStatus.Verified);
        result.VerifiedAt.Should().NotBeNull();
        result.OcrData.Should().Be("{\"name\":\"João\"}");
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentIsRejected_ShouldMapRejectionReason()
    {
        // Arrange
        var document = new DocumentBuilder().AsProofOfResidence().WithFileName("proof.pdf").WithFileUrl("storage/proof.pdf").Build();
        document.MarkAsPendingVerification();
        document.MarkAsRejected("Document is blurry");

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var query = new GetDocumentByIdQuery(document.Id);

        // Act
        var result = await _handler.HandleAsync(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(EDocumentStatus.Rejected);
        result.RejectionReason.Should().Be("Document is blurry");
    }
}
