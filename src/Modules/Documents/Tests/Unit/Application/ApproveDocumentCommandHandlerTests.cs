using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class ApproveDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger;
    private readonly ApproveDocumentCommandHandler _handler;

    public ApproveDocumentCommandHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApproveDocumentCommandHandler>>();

        _handler = new ApproveDocumentCommandHandler(
            _mockRepository.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    private void SetupAuthenticatedAdmin()
    {
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task HandleAsync_WithValidDocument_ShouldApproveDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var verificationNotes = "Documento válido e legível";

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ApproveDocumentCommand(documentId, verificationNotes);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Verified);
        document.OcrData.Should().Contain("\"notes\"");
        document.OcrData.Should().NotBeNullOrEmpty();

        _mockRepository.Verify(
            x => x.UpdateAsync(It.Is<Document>(d => d.Status == EDocumentStatus.Verified), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var command = new ApproveDocumentCommand(documentId, "Notes");

        SetupAuthenticatedAdmin();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Document with id {documentId} was not found");

        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotInPendingVerification_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        // Documento em status Uploaded (não PendingVerification)
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");

        SetupAuthenticatedAdmin();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, "Notes");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);

        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithNullVerificationNotes_ShouldStillSucceed()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        _mockRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ApproveDocumentCommand(documentId, null);

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Verified);
    }

    [Fact]
    public async Task HandleAsync_WithNonAdminUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "blob-key-123");
        document.MarkAsPendingVerification();

        // Setup non-admin user
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "provider")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        _mockRepository
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);

        var command = new ApproveDocumentCommand(documentId, "Notes");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>()
            .WithMessage("Only administrators can approve documents");

        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
