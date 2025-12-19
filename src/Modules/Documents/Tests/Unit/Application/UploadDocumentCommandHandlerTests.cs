using System.Linq.Expressions;
using System.Security.Claims;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.DTOs;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Application.Options;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Shared.Constants;
using MeAjudaAi.Shared.Jobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MeAjudaAi.Modules.Documents.Tests.Unit.Application;

public class UploadDocumentCommandHandlerTests
{
    private readonly Mock<IDocumentRepository> _mockRepository;
    private readonly Mock<IBlobStorageService> _mockBlobStorage;
    private readonly Mock<IBackgroundJobService> _mockJobService;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<IOptions<DocumentUploadOptions>> _mockUploadOptions;
    private readonly Mock<ILogger<UploadDocumentCommandHandler>> _mockLogger;
    private readonly UploadDocumentCommandHandler _handler;

    public UploadDocumentCommandHandlerTests()
    {
        _mockRepository = new Mock<IDocumentRepository>();
        _mockBlobStorage = new Mock<IBlobStorageService>();
        _mockJobService = new Mock<IBackgroundJobService>();
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockUploadOptions = new Mock<IOptions<DocumentUploadOptions>>();
        _mockLogger = new Mock<ILogger<UploadDocumentCommandHandler>>();

        // Configure default upload options
        _mockUploadOptions.Setup(x => x.Value).Returns(new DocumentUploadOptions
        {
            MaxFileSizeBytes = 10 * 1024 * 1024, // 10MB
            AllowedContentTypes = ["image/jpeg", "image/png", "image/jpg", "application/pdf"]
        });

        // Configure default behavior for background job service
        _mockJobService
            .Setup(x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()))
            .Returns(Task.CompletedTask);

        _handler = new UploadDocumentCommandHandler(
            _mockRepository.Object,
            _mockBlobStorage.Object,
            _mockJobService.Object,
            _mockHttpContextAccessor.Object,
            _mockUploadOptions.Object,
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
    public async Task HandleAsync_WithValidCommand_ShouldCreateDocument()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);
        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        var uploadUrl = "https://storage/upload-url";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((uploadUrl, expiresAt));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().NotBeEmpty();
        result.UploadUrl.Should().Be(uploadUrl);
        result.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));

        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Document>(d =>
                d.ProviderId == providerId &&
                d.DocumentType == EDocumentType.IdentityDocument &&
                d.FileName == "test.pdf" &&
                d.Status == EDocumentStatus.Uploaded),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockRepository.Verify(
            x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithProofOfResidence_ShouldCreateCorrectDocumentType()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "ProofOfResidence",
            "proof.pdf",
            "application/pdf",
            51200);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("upload", DateTime.UtcNow.AddHours(1)));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Document>(d => d.DocumentType == EDocumentType.ProofOfResidence), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateBlobStorageUrl()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var fileName = "criminal-record.pdf";
        var command = new UploadDocumentCommand(
            providerId,
            "CriminalRecord",
            fileName,
            "application/pdf",
            204800);

        var uploadUrl = "upload-url";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), "application/pdf", It.IsAny<CancellationToken>()))
            .ReturnsAsync((uploadUrl, expiresAt));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadUrl.Should().Be(uploadUrl);
        result.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));

        _mockBlobStorage.Verify(
            x => x.GenerateUploadUrlAsync(
                It.IsAny<string>(),
                "application/pdf",
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithOversizedFile_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "large.pdf",
            "application/pdf",
            11 * 1024 * 1024); // 11MB, excede o limite de 10MB

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("10MB");
    }

    [Theory]
    [InlineData("text/plain")]
    [InlineData("application/exe")]
    [InlineData("text/html")]
    public async Task HandleAsync_WithInvalidContentType_ShouldThrowArgumentException(string contentType)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            contentType,
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("not allowed");
    }

    [Fact]
    public async Task HandleAsync_WithContentTypeParameters_ShouldAcceptMediaType()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf; charset=utf-8", // Content-Type with parameters
            102400);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("url", DateTime.UtcNow.AddHours(1)));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UploadUrl.Should().NotBeEmpty();

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidDocumentType_ShouldThrowArgumentException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "InvalidType",
            "test.pdf",
            "application/pdf",
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("Invalid document type");
    }

    [Fact]
    public async Task HandleAsync_WithUnauthorizedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        SetupAuthenticatedUser(differentUserId); // User doesn't match provider

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("not authorized");

        // Verify warning was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("attempted to upload")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithAdminUser_ShouldAllowUploadForAnyProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        SetupAuthenticatedUser(adminUserId, "admin"); // Admin user with different ID

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        var uploadUrl = "https://storage/upload-url";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((uploadUrl, expiresAt));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Should succeed even though admin user ID != provider ID
        result.Should().NotBeNull();
        result.DocumentId.Should().NotBeEmpty();
        result.UploadUrl.Should().Be(uploadUrl);

        _mockRepository.Verify(
            x => x.AddAsync(It.Is<Document>(d => d.ProviderId == providerId), It.IsAny<CancellationToken>()),
            Times.Once);

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSystemAdminUser_ShouldAllowUploadForAnyProvider()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var systemAdminUserId = Guid.NewGuid();
        SetupAuthenticatedUser(systemAdminUserId, "system-admin"); // System admin

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        var uploadUrl = "https://storage/upload-url";
        var expiresAt = DateTime.UtcNow.AddHours(1);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((uploadUrl, expiresAt));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockRepository
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Should succeed for system-admin role
        result.Should().NotBeNull();
        result.DocumentId.Should().NotBeEmpty();

        _mockJobService.Verify(
            x => x.EnqueueAsync<IDocumentVerificationService>(
                It.IsAny<Expression<Func<IDocumentVerificationService, Task>>>(),
                It.IsAny<TimeSpan?>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullHttpContext_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var command = new UploadDocumentCommand(
            Guid.NewGuid(),
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("HTTP context not available");
    }

    [Fact]
    public async Task HandleAsync_WithUnauthenticatedUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal() // Sem identity autenticada
        };
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var command = new UploadDocumentCommand(
            Guid.NewGuid(),
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("not authenticated");
    }

    [Fact]
    public async Task HandleAsync_WithMissingUserIdClaim_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, "provider") // Sem claim 'sub' ou 'id'
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var command = new UploadDocumentCommand(
            Guid.NewGuid(),
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("User ID not found");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task HandleAsync_WithEmptyContentType_ShouldThrowArgumentException(string? contentType)
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            contentType!,
            102400);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("Content-Type is required");
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        SetupAuthenticatedUser(providerId);

        var command = new UploadDocumentCommand(
            providerId,
            "IdentityDocument",
            "test.pdf",
            "application/pdf",
            102400);

        _mockBlobStorage
            .Setup(x => x.GenerateUploadUrlAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("url", DateTime.UtcNow.AddHours(1)));

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        exception.Message.Should().Contain("Failed to upload document");
        exception.InnerException.Should().NotBeNull();
        exception.InnerException!.Message.Should().Contain("Database error");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unexpected error")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
