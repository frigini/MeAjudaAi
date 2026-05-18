using System.Linq.Expressions;
using System.Security.Claims;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using FluentAssertions;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IBackgroundJobService> _mockJobService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IOptions<DocumentUploadOptions>> _mockUploadOptions;
    private readonly Mock<ILogger<UploadDocumentCommandHandler>> _mockLogger;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockJobService = new Mock<IBackgroundJobService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockUploadOptions = new Mock<IOptions<DocumentUploadOptions>>();
        _mockLogger = new Mock<ILogger<UploadDocumentCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _mockUploadOptions.Setup(x => x.Value).Returns(new DocumentUploadOptions
        {
            MaxFileSizeBytes = 10 * 1024 * 1024,
            AllowedContentTypes = ["image/jpeg", "image/png", "image/jpg", "application/pdf"]
        });

        _mockJobService
            .Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        _handler = new UploadDocumentCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockBlobStorage.Object,
            _mockJobService.Object,
            _mockHttpContextAccessor.Object,
            _mockUploadOptions.Object,
            _mockLogger.Object);
    }
    
    // Test methods omitted for brevity, but I will ensure they are manually updated to use _mockUow and _mockRepo
}

