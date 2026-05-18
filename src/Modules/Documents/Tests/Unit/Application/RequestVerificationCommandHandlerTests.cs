using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Queries;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Jobs;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class RequestVerificationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IBackgroundJobService> _mockJobService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<RequestVerificationCommandHandler>> _mockLogger;
    private readonly RequestVerificationCommandHandler _handler;

    public RequestVerificationCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockJobService = new Mock<IBackgroundJobService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<RequestVerificationCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);

        _handler = new RequestVerificationCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockJobService.Object,
            _mockHttpContextAccessor.Object,
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
    public async Task HandleAsync_WithExistingDocument_ShouldMarkAsPendingVerificationAndEnqueueJob()
    {
        var documentId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "identity.pdf", "blob-key-123");
        
        SetupAuthenticatedUser(providerId);

        _mockQueries.Setup(x => x.GetByIdAsync(documentId, It.IsAny<CancellationToken>())).ReturnsAsync(document);
        _mockUow.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var command = new RequestVerificationCommand(documentId);
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();
        document.Status.Should().Be(EDocumentStatus.PendingVerification);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockJobService.Verify(x => x.EnqueueAsync<IDocumentVerificationService>(It.IsAny<System.Linq.Expressions.Expression<Func<IDocumentVerificationService, Task>>>(), It.IsAny<TimeSpan?>()), Times.Once);
    }
}
