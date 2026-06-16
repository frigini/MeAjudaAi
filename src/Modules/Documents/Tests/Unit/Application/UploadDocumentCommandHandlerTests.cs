using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Database.Outbox;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IRepository<Document, Guid>> _mockRepo;
    private readonly Mock<IRepository<OutboxMessage, Guid>> _mockOutboxRepo;
    private readonly Mock<IDocumentQueries> _mockQueries;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly DocumentUploadOptions _uploadOptions;
    private readonly Mock<ILogger<UploadDocumentCommandHandler>> _mockLogger;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockRepo = new Mock<IRepository<Document, Guid>>();
        _mockOutboxRepo = new Mock<IRepository<OutboxMessage, Guid>>();
        _mockQueries = new Mock<IDocumentQueries>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _uploadOptions = new DocumentUploadOptions
        {
            MaxFileSizeBytes = 10 * 1024 * 1024,
            AllowedContentTypes = ["image/jpeg", "image/png", "image/jpg", "application/pdf"]
        };
        _mockLogger = new Mock<ILogger<UploadDocumentCommandHandler>>();

        _mockUow.Setup(x => x.GetRepository<Document, Guid>()).Returns(_mockRepo.Object);
        _mockUow.Setup(x => x.GetRepository<OutboxMessage, Guid>()).Returns(_mockOutboxRepo.Object);

        _handler = new UploadDocumentCommandHandler(
            _mockUow.Object,
            _mockQueries.Object,
            _mockBlobStorage.Object,
            _mockHttpContextAccessor.Object,
            _uploadOptions,
            _mockLogger.Object);
    }
    
    private void SetupAuthenticatedUser(Guid? userId, string role = "provider", bool isAuthenticated = true)
    {
        var claims = new List<Claim>();
        if (userId.HasValue)
        {
            claims.Add(new Claim(AuthConstants.Claims.Subject, userId.Value.ToString()));
        }
        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, isAuthenticated ? "TestAuth" : null);
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    }

    [Fact]
    public async Task HandleAsync_WithValidCommand_ShouldUploadAndEnqueue()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        _mockBlobStorage.Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload-url", DateTime.UtcNow.AddMinutes(15)));

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.UploadUrl.Should().Be("upload-url");

        _mockRepo.Verify(x => x.Add(It.IsAny<Document>()), Times.Once);
        _mockUow.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOutboxRepo.Verify(x => x.Add(It.Is<OutboxMessage>(m => 
            m.Type == OutboxMessageTypes.DocumentVerification && 
            m.Payload.Contains(result.DocumentId.ToString()))), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenHttpContextIsNull_ShouldThrowUnauthorized()
    {
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var command = new UploadDocumentCommand(Guid.NewGuid(), EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*HTTP context not available*");
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotAuthenticated_ShouldThrowUnauthorized()
    {
        SetupAuthenticatedUser(Guid.NewGuid(), isAuthenticated: false);

        var command = new UploadDocumentCommand(Guid.NewGuid(), EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*User is not authenticated*");
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdMissingFromToken_ShouldThrowUnauthorized()
    {
        SetupAuthenticatedUser(null);

        var command = new UploadDocumentCommand(Guid.NewGuid(), EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*User ID not found in token*");
    }

    [Fact]
    public async Task HandleAsync_WithDifferentProviderId_AndNotAdmin_ShouldThrowUnauthorized()
    {
        var providerId = Guid.NewGuid();
        var loggedUserId = Guid.NewGuid();
        SetupAuthenticatedUser(loggedUserId, role: "provider");

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<UnauthorizedAccessException>().WithMessage("*authorized*");
    }

    [Fact]
    public async Task HandleAsync_WithDifferentProviderId_AndIsAdmin_ShouldSucceed()
    {
        var providerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        SetupAuthenticatedUser(adminId, role: RoleConstants.Admin);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        _mockBlobStorage.Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload-url", DateTime.UtcNow.AddMinutes(15)));

        var result = await _handler.HandleAsync(command);

        result.Should().NotBeNull();
        result.UploadUrl.Should().Be("upload-url");
    }

    [Fact]
    public async Task HandleAsync_WithInvalidDocumentType_ShouldThrowArgumentException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, "InvalidType", "test.pdf", "application/pdf", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Invalid document type*");
    }

    [Fact]
    public async Task HandleAsync_WithFileTooLarge_ShouldThrowArgumentException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 20 * 1024 * 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*File too large*");
    }

    [Fact]
    public async Task HandleAsync_WithEmptyContentType_ShouldThrowArgumentException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*Content-Type is required*");
    }

    [Fact]
    public async Task HandleAsync_WithForbiddenContentType_ShouldThrowArgumentException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.exe", "application/x-msdownload", 1024);

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*File type not allowed*");
    }

    [Fact]
    public async Task HandleAsync_WhenUnexpectedExceptionOccurs_ShouldThrowInvalidOperationException()
    {
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(providerId, EDocumentType.IdentityDocument.ToString(), "test.pdf", "application/pdf", 1024);

        _mockBlobStorage.Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Storage service unavailable"));

        var act = async () => await _handler.HandleAsync(command);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*enviar*");
    }
}



