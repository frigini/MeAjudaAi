using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class RequestVerificationCommandHandlerTests
{
    private readonly Mock<IDocumentsUnitOfWork> _mockUow;
    private readonly Mock<IBackgroundJobService> _mockBackgroundJobService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RequestVerificationCommandHandler>> _mockLogger;
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests()
    {
        _mockUow = new Mock<IDocumentsUnitOfWork>();
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RequestVerificationCommandHandler>>();
        _handler = new RequestVerificationCommandHandler(
            _mockUow.Object,
            _mockBackgroundJobService.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldMarkAsPendingVerificationAndEnqueueJob()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBackgroundJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthorizedUser_ShouldReturnUnauthorized()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", differentUserId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(401);
    }

    [Theory]
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.LegacySuperAdmin)]
    public async Task HandleAsync_WithAdminUser_ShouldAllowVerificationForAnyDocument(string adminRole)
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", adminUserId.ToString()),
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, adminRole)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth", "sub", ClaimTypes.Role);
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        var documentId = Guid.NewGuid();
        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Failed to request verification. Please try again later.");
    }

    [Fact]
    public async Task HandleAsync_WithVerifiedDocument_ShouldReturnValidationError()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"verified\": true}");

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Verified");
    }

    [Fact]
    public async Task HandleAsync_WithPendingVerificationDocument_ShouldReturnValidationError()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        document.MarkAsPendingVerification();

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("PendingVerification");
    }

    [Fact]
    public async Task HandleAsync_WithFailedDocument_ShouldAllowRetry()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        document.MarkAsPendingVerification();
        document.MarkAsFailed("OCR service unavailable");

        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull("rejection reason should be cleared on retry");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBackgroundJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }
}
