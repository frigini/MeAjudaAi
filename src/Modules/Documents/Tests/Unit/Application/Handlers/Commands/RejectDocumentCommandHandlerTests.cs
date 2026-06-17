using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application.Handlers.Commands;

public class RejectDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RejectDocumentCommandHandler>> _mockLogger;
    private readonly RejectDocumentCommandHandler _handler;

    public RejectDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RejectDocumentCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _handler = new RejectDocumentCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
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

    [Theory]
    [InlineData(RoleConstants.Admin)]
    public async Task HandleAsync_WithAdminUser_ShouldRejectDocument(string adminRole)
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser(adminRole);

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var command = new RejectDocumentCommand(documentId, "Documento ilegível");
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Rejected);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(RoleConstants.Customer)]
    [InlineData(RoleConstants.Provider)]
    public async Task HandleAsync_WithNonAdminUser_ShouldReturnFailure(string nonAdminRole)
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser(nonAdminRole);
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "Reason");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenDocumentNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        SetupAuthenticatedUser(RoleConstants.Admin);
        
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync((Document)null!);

        var command = new RejectDocumentCommand(documentId, "Reason");

        // Act
        var act = async () => await _handler.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsNotPendingVerification_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        // Status defaults to Uploaded

        SetupAuthenticatedUser(RoleConstants.Admin);
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "Reason");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("BadRequest");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenReasonIsMissing_ShouldReturnFailure()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        document.MarkAsPendingVerification();

        SetupAuthenticatedUser(RoleConstants.Admin);
        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);

        var command = new RejectDocumentCommand(documentId, "");

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("BadRequest");
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}



