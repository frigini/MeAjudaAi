using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class ApproveDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger;
    private readonly ApproveDocumentCommandHandler _handler;

    public ApproveDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApproveDocumentCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _handler = new ApproveDocumentCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    private HttpContext CreateAuthenticatedAdminContext()
    {
        var claims = new List<Claim>
        {
            new Claim("sub", Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, RoleConstants.Admin)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        return httpContext;
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldApproveDocument()
    {
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "https://storage/doc.pdf");
        document.MarkAsPendingVerification();
        var command = new ApproveDocumentCommand(document.Id, "Verified OK");

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.Verified);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        var documentId = Guid.NewGuid();
        var command = new ApproveDocumentCommand(documentId, null);

        _mockQueries
            .Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());

        var act = () => _handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Document*not found*");
    }

    [Fact]
    public async Task Handle_WithAlreadyApprovedDocument_ShouldReturnBadRequest()
    {
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "https://storage/doc.pdf");
        var statusProperty = typeof(Document).GetProperty("Status");
        statusProperty?.SetValue(document, EDocumentStatus.Verified);
        var command = new ApproveDocumentCommand(document.Id, null);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("status");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrows_ShouldReturnGenericFailure()
    {
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "test.pdf", "https://storage/doc.pdf");
        document.MarkAsPendingVerification();
        var command = new ApproveDocumentCommand(document.Id, null);

        _mockQueries
            .Setup(x => x.GetByIdAsync(document.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(document);
        _mockHttpContextAccessor
            .Setup(x => x.HttpContext)
            .Returns(CreateAuthenticatedAdminContext());
        _mockUow
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Message.Should().Contain("Falha ao aprovar");
    }
}