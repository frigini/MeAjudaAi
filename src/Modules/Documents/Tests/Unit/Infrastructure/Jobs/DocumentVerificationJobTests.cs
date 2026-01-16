using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Jobs;

public sealed class DocumentVerificationJobTests
{
    private readonly Mock<IDocumentRepository> _repositoryMock;
    private readonly Mock<IDocumentIntelligenceService> _intelligenceMock;
    private readonly Mock<IBlobStorageService> _blobStorageMock;
    private readonly IConfiguration _configuration;
    private readonly Mock<ILogger<DocumentVerificationJob>> _loggerMock;
    private readonly DocumentVerificationJob _job;

    public DocumentVerificationJobTests()
    {
        _repositoryMock = new Mock<IDocumentRepository>();
        _intelligenceMock = new Mock<IDocumentIntelligenceService>();
        _blobStorageMock = new Mock<IBlobStorageService>();
        _loggerMock = new Mock<ILogger<DocumentVerificationJob>>();

        // Usar ConfigurationBuilder real para valores padrão
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documents:Verification:MinimumConfidence"] = "0.7"
            })
            .Build();

        _job = new DocumentVerificationJob(
            _repositoryMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);
    }

    [Fact]
    public void Constructor_WithNullRepository_ShouldThrow()
    {
        // Act
        var act = () => new DocumentVerificationJob(
            null!,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("documentRepository");
    }

    [Fact]
    public void Constructor_WithNullIntelligenceService_ShouldThrow()
    {
        // Act
        var act = () => new DocumentVerificationJob(
            _repositoryMock.Object,
            null!,
            _blobStorageMock.Object,
            _configuration,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("documentIntelligenceService");
    }

    [Fact]
    public void Constructor_WithNullBlobStorage_ShouldThrow()
    {
        // Act
        var act = () => new DocumentVerificationJob(
            _repositoryMock.Object,
            _intelligenceMock.Object,
            null!,
            _configuration,
            _loggerMock.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("blobStorageService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrow()
    {
        // Act
        var act = () => new DocumentVerificationJob(
            _repositoryMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            _configuration,
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
    }

    [Fact]
    public async Task ProcessDocumentAsync_WhenDocumentNotFound_ShouldLogWarning()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
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
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, status);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        _blobStorageMock.Verify(b => b.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _intelligenceMock.Verify(i => i.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WhenBlobNotFound_ShouldMarkAsFailed()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Failed);
        _repositoryMock.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithSuccessfulOcr_ShouldMarkAsVerified()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);
        var extractedData = new Dictionary<string, string> { ["cpf"] = "12345678900" };

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
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

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Verified);
        _repositoryMock.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithLowConfidence_ShouldMarkAsRejected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
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

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Rejected);
        _repositoryMock.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithOcrFailure_ShouldMarkAsRejected()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
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

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Rejected);
        _repositoryMock.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithTransientException_ShouldRethrow()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var act = () => _job.ProcessDocumentAsync(documentId);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithPermanentException_ShouldMarkAsFailed()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.Uploaded);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _blobStorageMock.Setup(b => b.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Permanent error"));

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Failed);
        _repositoryMock.Verify(r => r.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(EDocumentStatus.Uploaded)]
    [InlineData(EDocumentStatus.PendingVerification)]
    [InlineData(EDocumentStatus.Failed)]
    public async Task ProcessDocumentAsync_ShouldMarkAsVerified_WhenOcrSucceedsWithHighConfidence(EDocumentStatus initialStatus)
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, initialStatus);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
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

        // Act
        await _job.ProcessDocumentAsync(documentId);

        // Assert
        document.Status.Should().Be(EDocumentStatus.Verified);
    }

    [Fact]
    public async Task ProcessDocumentAsync_WithCustomMinimumConfidence_ShouldUseConfiguredValue()
    {
        // Arrange
        var customConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documents:Verification:MinimumConfidence"] = "0.9"
            })
            .Build();

        var customJob = new DocumentVerificationJob(
            _repositoryMock.Object,
            _intelligenceMock.Object,
            _blobStorageMock.Object,
            customConfig,
            _loggerMock.Object);

        var documentId = Guid.NewGuid();
        var document = CreateDocument(documentId, EDocumentStatus.PendingVerification);

        _repositoryMock.Setup(r => r.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _blobStorageMock.Setup(b => b.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _blobStorageMock.Setup(b => b.GenerateDownloadUrlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("https://fake-url", DateTime.UtcNow.AddHours(1)));

        // Confiança de 0.85 seria aceita com threshold padrão (0.7), mas rejeitada com 0.9
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

        // Act
        await customJob.ProcessDocumentAsync(documentId);

        // Assert - Deve ser rejeitado porque 0.85 < 0.9
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

        // Use reflection to set status since it's protected
        var statusProperty = typeof(Document).GetProperty(nameof(Document.Status));
        statusProperty?.SetValue(document, status);

        return document;
    }
}
