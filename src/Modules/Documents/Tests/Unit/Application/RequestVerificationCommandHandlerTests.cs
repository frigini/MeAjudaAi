using System.Security.Claims;
using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Database.Outbox;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MeAjudaAi.Shared.Utilities.Constants;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class RequestVerificationCommandHandlerTests
{
    private readonly Mock<IDocumentsUnitOfWork> _mockUow;
    private readonly Mock<IDocumentQueries> _mockDocumentQueries;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RequestVerificationCommandHandler>> _mockLogger;
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests()
    {
        _mockUow = new Mock<IDocumentsUnitOfWork>();
        _mockDocumentQueries = new Mock<IDocumentQueries>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RequestVerificationCommandHandler>>();
        _handler = new RequestVerificationCommandHandler(
            _mockUow.Object,
            _mockDocumentQueries.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task HandleAsync_WithExistingDocument_ShouldMarkAsPendingVerificationAndCreateOutboxMessage()
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

        var mockOutboxRepo = new Mock<IRepository<OutboxMessage, Guid>>();
        mockOutboxRepo.Setup(r => r.Add(It.IsAny<OutboxMessage>()));

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.GetRepository<OutboxMessage, Guid>()).Returns(mockOutboxRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockOutboxRepo.Verify(r => r.Add(It.Is<OutboxMessage>(m => m.Type == OutboxMessageTypes.DocumentVerification)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithUnauthorizedUser_ShouldReturnNotFound()
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
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, differentUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
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
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var mockOutboxRepo = new Mock<IRepository<OutboxMessage, Guid>>();
        mockOutboxRepo.Setup(r => r.Add(It.IsAny<OutboxMessage>()));

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.GetRepository<OutboxMessage, Guid>()).Returns(mockOutboxRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Name, "test-user")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentNotFound_ShouldReturnNotFound()
    {
        var documentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim>
        {
            new Claim("sub", userId.ToString()),
            new Claim(ClaimTypes.Name, "test-user"),
            new Claim(ClaimTypes.Role, RoleConstants.Admin)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        httpContext.User = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        
        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error!.StatusCode.Should().Be(404);
        result.Error.Message.Should().Contain("não encontrado");
    }

    [Fact]
    public async Task HandleAsync_WhenSaveChangesThrows_ShouldReturnInternal()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "blob-url");
        
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim("sub", providerId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(It.IsAny<DocumentId>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("db error"));

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsVerified_ShouldReturnBadRequest()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "blob-url");
        document.MarkAsPendingVerification(); // Status = PendingVerification
        document.MarkAsVerified(null); // Status = Verified
        
        var httpContext = new DefaultHttpContext();
        var claims = new List<Claim> { new Claim("sub", providerId.ToString()) };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(It.IsAny<DocumentId>(), It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, default);

        result.IsFailure.Should().BeTrue();
        result.Error!.StatusCode.Should().Be(400);
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
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

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
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

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

        var mockOutboxRepo = new Mock<IRepository<OutboxMessage, Guid>>();
        mockOutboxRepo.Setup(r => r.Add(It.IsAny<OutboxMessage>()));

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.GetRepository<OutboxMessage, Guid>()).Returns(mockOutboxRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _mockDocumentQueries.Setup(x => x.GetByIdAndProviderAsync(documentId, providerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new RequestVerificationCommand(documentId);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        document.RejectionReason.Should().BeNull("rejection reason should be cleared on retry");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
