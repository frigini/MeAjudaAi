using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class RejectDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RejectDocumentCommandHandler>> _mockLogger;
    private readonly RejectDocumentCommandHandler _handler;

    public RejectDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RejectDocumentCommandHandler>>();

        _handler = new RejectDocumentCommandHandler(
            _mockUow.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    private void SetupAuthenticatedUser(string role)
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, role)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    private void SetupAuthenticatedAdmin() => SetupAuthenticatedUser(RoleConstants.Admin);

    [Theory]
    [InlineData(RoleConstants.Admin)]
    [InlineData(RoleConstants.SystemAdmin)]
    [InlineData(RoleConstants.LegacySystemAdmin)]
    [InlineData(RoleConstants.SuperAdmin)]
    [InlineData(RoleConstants.LegacySuperAdmin)]
    public async Task HandleAsync_WithAdminUser_ShouldRejectDocument(string adminRole)
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var rejectionReason = "Documento ilegível";

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser(adminRole);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RejectDocumentCommand(documentId, rejectionReason);
        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Rejected);
        document.RejectionReason.Should().Be(rejectionReason);

        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        var documentId = Guid.NewGuid();
        var command = new RejectDocumentCommand(documentId, "Invalid");

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Document with id {documentId} was not found");
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotInPendingVerification_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RejectDocumentCommand(documentId, "Invalid");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRejectionReason_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RejectDocumentCommand(documentId, "");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Motivo de recusa é obrigatório");
    }

    [Fact]
    public async Task HandleAsync_WithNonAdminUser_ShouldThrowForbiddenAccessException()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "provider")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new RejectDocumentCommand(documentId, "Invalid");
        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ForbiddenAccessException>().WithMessage("Only administrators can reject documents");
    }
}