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

public class ApproveDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger;
    private readonly ApproveDocumentCommandHandler _handler;

    public ApproveDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApproveDocumentCommandHandler>>();

        _handler = new ApproveDocumentCommandHandler(
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
    public async Task HandleAsync_WithAdminUser_ShouldApproveDocument(string adminRole)
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var verificationNotes = "Documento válido e legível";

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser(adminRole);

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ApproveDocumentCommand(documentId, verificationNotes);

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Verified);
        
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        var documentId = Guid.NewGuid();
        var command = new ApproveDocumentCommand(documentId, "Notes");

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Document with id {documentId} was not found");
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotInPendingVerification_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new ApproveDocumentCommand(documentId, "Notes");

        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task HandleAsync_WithNullVerificationNotes_ShouldStillSucceed()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new ApproveDocumentCommand(documentId, null);

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Verified);
        document.OcrData.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNonAdminUser_ShouldThrowForbiddenAccessException()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser("provider");

        var mockRepo = new Mock<IRepository<Document, DocumentId>>();
        mockRepo.Setup(r => r.TryFindAsync(new DocumentId(documentId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockUow.Setup(x => x.GetRepository<Document, DocumentId>()).Returns(mockRepo.Object);

        var command = new ApproveDocumentCommand(documentId, "Notes");

        var act = async () => await _handler.HandleAsync(command);
// Assert
await act.Should().ThrowAsync<ForbiddenAccessException>()
    .WithMessage("Apenas administradores podem aprovar documentos");
}

}