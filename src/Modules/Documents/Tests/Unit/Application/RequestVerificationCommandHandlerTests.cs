using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class RequestVerificationCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IBackgroundJobService> _mockBackgroundJobService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RequestVerificationCommandHandler>> _mockLogger;
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RequestVerificationCommandHandler>>();
        _handler = new RequestVerificationCommandHandler(
            _mockRepository.Object,
            _mockBackgroundJobService.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldMarkAsPendingVerificationAndEnqueueJob()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Setup authentication - user matches provider ID
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        _mockRepository.Verify(x => x.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBackgroundJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthorizedUser_ShouldReturnUnauthorized()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Setup authentication - user does NOT match provider ID and is not admin
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", differentUserId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(401);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithAdminUser_ShouldAllowVerificationForAnyDocument()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Setup authentication - user is admin
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", adminUserId.ToString()),
            new Claim(ClaimTypes.Name, "admin-user"),
            new Claim(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldReturnFailure()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrows_ShouldReturnFailureResult()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Message.Should().Be("Failed to request verification. Please try again later.");
    }

    [Fact]
    public async Task HandleAsync_WithVerifiedDocument_ShouldReturnValidationError()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Mark document as verified
        document.MarkAsPendingVerification();
        document.MarkAsVerified("{\"verified\": true}");

        // Setup authentication - user matches provider ID
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Verified");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithPendingVerificationDocument_ShouldReturnValidationError()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Mark document as pending verification
        document.MarkAsPendingVerification();

        // Setup authentication - user matches provider ID
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("PendingVerification");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithFailedDocument_ShouldAllowRetry()
    {
        // Preparação
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "test-blob-url");

        // Mark document as failed
        document.MarkAsPendingVerification();
        document.MarkAsFailed("OCR service unavailable");

        // Setup authentication - user matches provider ID
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", providerId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockBackgroundJobService.Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        var command = new RequestVerificationCommand(documentId);

        // Ação
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Verificação
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull("rejection reason should be cleared on retry");
        _mockRepository.Verify(x => x.UpdateAsync(document, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockBackgroundJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(
            It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }
}
