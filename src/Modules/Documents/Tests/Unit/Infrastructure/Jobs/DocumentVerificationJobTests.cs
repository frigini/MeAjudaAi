using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Infrastructure.Jobs;
using MeAjudaAi.Shared.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Jobs;

public sealed class DocumentVerificationJobTests
{
    private readonly Mock<IDocumentsUnitOfWork> _uowMock;
    private readonly Mock<IDocumentIntelligenceService> _intelligenceMock;
    private readonly Mock<IBlobStorageService> _blobStorageMock;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<DocumentVerificationJob>> _loggerMock;
    private readonly DocumentVerificationJob _job;

    public DocumentVerificationJobTests()
    {
        _uowMock = new Mock<IDocumentsUnitOfWork>();
        _intelligenceMock = new Mock<IDocumentIntelligenceService>();
        _blobStorageMock = new Mock<IBlobStorageService>();
        _loggerMock = new Mock<ILogger<DocumentVerificationJob>>();

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documents:Verification:MinimumConfidence"] = "0.7"
            })
            .Build();

        _job = new DocumentVerificationJob(
            _uowMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullUow_ShouldThrow()
    {
        var act = () => new DocumentVerificationJob(
            null!,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("uow");
    }

    [Fact]
    public void Constructor_WithNullIntelligenceService_ShouldThrow()
    {
        var act = () => new DocumentVerificationJob(
            _uowMock.Object,
            null!,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("documentIntelligenceService");
    }

    [Fact]
    public void Constructor_WithNullBlobStorage_ShouldThrow()
    {
        var act = () => new DocumentVerificationJob(
            _uowMock.Object,
            _intelligenceMock.Object,
            null!,
            _configuration,
            _loggerMock.Object);

        act.Should().Throw<ArgumentNullException>().WithParameterName("blobStorageService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        var act = () => new DocumentVerificationJob(
            _uowMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            null!);

        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ProcessDocumentAsync_WhenDocumentNotFound_ShouldLogWarning()
    {
        var documentId = Guid.NewGuid();
        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        await _job.ProcessDocumentAsync(documentId);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Theory]
    [InlineData(EDocumentStatus.Verified)]
    [InlineData(EDocumentStatus.Rejected)]
    public async Task ProcessDocumentAsync_WhenDocumentAlreadyProcessed_ShouldSkip(EDocumentStatus status)
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, status);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        await _job.ProcessDocumentAsync(documentId);

        _blobStorageMock.Verify(b => b.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _intelligenceMock.Verify(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WhenBlobNotFound_ShouldMarkAsFailed()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Failed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithSuccessfulOcr_ShouldMarkAsVerified()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);
        var extractedData = new Dictionary<string, string> { ["cpf"] = "12345678900" };

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://blob.url/file.pdf", DateTime.UtcNow.AddHours(1)));
        _intelligenceMock.Setup(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(
                Success: true,
                ExtractedData: "{\"cpf\":\"12345678900\"}",
                Fields: extractedData,
                Confidence: 0.95f,
                ErrorMessage: null));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Verified);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithLowConfidence_ShouldMarkAsRejected()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://blob.url/file.pdf", DateTime.UtcNow.AddHours(1)));
        _intelligenceMock.Setup(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(
                Success: true,
                ExtractedData: "{}",
                Fields: new Dictionary<string, string>(),
                Confidence: 0.5f,
                ErrorMessage: null));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Rejected);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithOcrFailure_ShouldMarkAsRejected()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://blob.url/file.pdf", DateTime.UtcNow.AddHours(1)));
        _intelligenceMock.Setup(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(
                Success: false,
                ExtractedData: null,
                Fields: null,
                Confidence: null,
                ErrorMessage: "OCR failed"));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Rejected);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithTransientException_ShouldRethrow()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        var act = () => _job.ProcessDocumentAsync(documentId);

        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithPermanentException_ShouldMarkAsFailed()
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Permanent error"));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Failed);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EDocumentStatus.Uploaded)]
    [InlineData(EDocumentStatus.PendingVerification)]
    [InlineData(EDocumentStatus.Failed)]
    public async Task ProcessDocumentAsync_ShouldMarkAsVerified_WhenOcrSucceedsWithHighConfidence(EDocumentStatus initialStatus)
    {
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, initialStatus);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://blob.url/file.pdf", DateTime.UtcNow.AddHours(1)));
        _intelligenceMock.Setup(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(
                Success: true,
                ExtractedData: "{}",
                Fields: new Dictionary<string, string>(),
                Confidence: 0.95f,
                ErrorMessage: null));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Verified);
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithCustomMinimumConfidence_ShouldUseConfiguredValue()
    {
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documents:Verification:MinimumConfidence"] = "0.9"
            })
            .Build();

        var customJob = new DocumentVerificationJob(
            _uowMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            customConfig,
            _loggerMock.Object);

        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.PendingVerification);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _uowMock.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _blobStorageMock.Setup(b => b.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://fake-url", DateTime.UtcNow.AddHours(1)));

        _intelligenceMock.Setup(i => i.AnalyzeDocumentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(
                Success: true,
                ExtractedData: "{\"name\":\"Test\"}",
                Fields: new Dictionary<string, string> { { "name", "Test" } },
                Confidence: 0.85f,
                ErrorMessage: null));

        await customJob.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Rejected);
        document.RejectionReason.Should().Contain("85");
    }

    private static Document CreateDocument(Guid id, EDocumentStatus status)
    {
        var document = Document.Create(
            providerId: Guid.NewGuid(),
            documentType: EDocumentType.IdentityDocument,
            fileName: "test.pdf",
            fileUrl: "test-file.pdf");

        var statusProperty = typeof(Document).GetProperty(nameof(Document.Status));
        statusProperty?.SetValue(document, status);

        return document;
    }
}
