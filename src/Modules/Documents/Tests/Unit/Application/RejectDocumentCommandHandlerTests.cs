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

public class RejectDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RejectDocumentCommandHandler>> _mockLogger;
    private readonly RejectDocumentCommandHandler _handler;

    public RejectDocumentCommandHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RejectDocumentCommandHandler>>();

        _handler = new RejectDocumentCommandHandler(
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
    public async Task HandleAsync_WithValidDocument_ShouldRejectDocument()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var rejectionReason = "Documento ilegível";

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var command = new RejectDocumentCommand(documentId, rejectionReason);
        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Rejected);
        document.RejectionReason.Should().Be(rejectionReason);

        _mockRepository.Verify(x => x.UpdateAsync(It.Is<Document>(d => d.Status == EDocumentStatus.Rejected), It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        var documentId = Guid.NewGuid();
        var command = new RejectDocumentCommand(documentId, "Invalid");

        SetupAuthenticatedAdmin();

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Document with id {documentId} was not found");

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithDocumentNotInPendingVerification_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");

        SetupAuthenticatedAdmin();

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "Invalid");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WithEmptyRejectionReason_ShouldReturnFailure()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedAdmin();

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);
        result.Error.Message.Should().Contain("Motivo de recusa é obrigatório");

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
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

        _mockRepository.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "Invalid");
        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ForbiddenAccessException>().WithMessage("Only administrators can reject documents");

        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
