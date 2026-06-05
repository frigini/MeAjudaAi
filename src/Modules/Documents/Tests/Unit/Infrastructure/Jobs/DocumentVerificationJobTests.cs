using MeAjudaAi.Shared.Database.Abstractions;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Infrastructure.Jobs;

public class DocumentVerificationJobTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IDocumentIntelligenceService> _mockIntelligenceService;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<ILogger<MeAjudaAi.Modules.Documents.Infrastructure.Jobs.DocumentVerificationJob>> _mockLogger;
    private readonly MeAjudaAi.Modules.Documents.Infrastructure.Jobs.DocumentVerificationJob _job;

    public DocumentVerificationJobTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockIntelligenceService = new Mock<IDocumentIntelligenceService>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockLogger = new Mock<ILogger<MeAjudaAi.Modules.Documents.Infrastructure.Jobs.DocumentVerificationJob>>();
        
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Documents:Verification:MinimumConfidence"] = "0.7"
            })
            .Build();
        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _job = new MeAjudaAi.Modules.Documents.Infrastructure.Jobs.DocumentVerificationJob(
            _mockUow.Object,
            _mockQueries.Object,
            _mockIntelligenceService.Object,
            _mockBlobStorage.Object,
            config,
            _mockLogger.Object);
    }

    [Fact]
    public async Task Should_skip_when_no_pending_documents()
    {
        var documentId = Guid.NewGuid();
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document)null!);

        await _job.ProcessDocumentAsync(documentId);

        _mockBlobStorage.Verify(x => x.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockIntelligenceService.Verify(x => x.AnalyzeDocumentAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Should_process_and_persist_verified_document()
    {
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockBlobStorage.Setup(x => x.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockBlobStorage.Setup(x => x.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>())).ReturnsAsync(("download-url", DateTime.UtcNow.AddMinutes(15)));
        
        _mockIntelligenceService.Setup(x => x.AnalyzeDocumentAsync("download-url", document.DocumentType.ToString(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OcrResult(true, "extracted-data", null, 0.9f, null));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Verified);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Should_handle_intelligence_service_failure()
    {
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockBlobStorage.Setup(x => x.ExistsAsync(document.FileUrl, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _mockBlobStorage.Setup(x => x.GenerateDownloadUrlAsync(document.FileUrl, It.IsAny<CancellationToken>())).ReturnsAsync(("download-url", DateTime.UtcNow.AddMinutes(15)));
        
        _mockIntelligenceService.Setup(x => x.AnalyzeDocumentAsync("download-url", document.DocumentType.ToString(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("API Error"));

        await _job.ProcessDocumentAsync(documentId);

        document.Status.Should().Be(EDocumentStatus.Failed);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}



