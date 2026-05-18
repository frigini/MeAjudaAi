using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Shared.Database;
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
        
        var mockConfig = new Mock<IConfiguration>();
        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _job = new MeAjudaAi.Modules.Documents.Infrastructure.Jobs.DocumentVerificationJob(
            _mockUow.Object,
            _mockQueries.Object,
            _mockIntelligenceService.Object,
            _mockBlobStorage.Object,
            mockConfig.Object,
            _mockLogger.Object);
    }
}
