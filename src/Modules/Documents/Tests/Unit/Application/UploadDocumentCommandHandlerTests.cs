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
    
    private void SetupAuthenticatedUser(Guid userId, string role = "provider")
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUploadAndEnqueue()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        _mockBlobStorage.Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload-url", DateTime.UtcNow.AddMinutes(15)));

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.UploadUrl.Should().Be("upload-url");

        _mockRepo.Verify(x => x.Add(It.IsAny<Document>()), Times.Once);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidContentType_ShouldThrowException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.exe", "application/x-msdownload", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>();

        _mockRepo.Verify(x => x.Add(It.IsAny<Document>()), Times.Never);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(), It.IsAny<TimeSpan?>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithFileTooLarge_ShouldThrowException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 20 * 1024 * 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>();

        _mockRepo.Verify(x => x.Add(It.IsAny<Document>()), Times.Never);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _mockJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(), It.IsAny<TimeSpan?>()), Times.Never);
    }
}

