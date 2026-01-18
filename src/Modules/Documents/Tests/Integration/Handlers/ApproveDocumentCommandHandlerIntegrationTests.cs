using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.Repositories;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence.Repositories;
using MeAjudaAi.Shared.Exceptions;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Handlers;

/// <summary>
/// Testes de integração para ApproveDocumentCommandHandler.
/// Valida integração real com banco de dados PostgreSQL via Testcontainers.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Application")]
public sealed class ApproveDocumentCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DocumentsDbContext? _dbContext;
    private IDocumentRepository? _repository;
    private ApproveDocumentCommandHandler? _handler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger;

    public ApproveDocumentCommandHandlerIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("documents_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApproveDocumentCommandHandler>>();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new DocumentsDbContext(options);
        await _dbContext.Database.MigrateAsync();

        _repository = new DocumentRepository(_dbContext);
        _handler = new ApproveDocumentCommandHandler(
            _repository,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }

        await _postgresContainer.DisposeAsync();
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
    public async Task HandleAsync_WithValidDocument_ShouldPersistApprovalToDatabase()
    {
        // Arrange
        SetupAuthenticatedAdmin();

        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "documents/identity.pdf");
        
        document.MarkAsPendingVerification();

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        var verificationNotes = "Documento aprovado - identificação válida";
        var command = new ApproveDocumentCommand(document.Id.Value, verificationNotes);

        // Act
        var result = await _handler!.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // Verificar persistência no banco
        _dbContext!.ChangeTracker.Clear();
        var persistedDocument = await _repository.GetByIdAsync(document.Id.Value);

        persistedDocument.Should().NotBeNull();
        persistedDocument!.Status.Should().Be(EDocumentStatus.Verified);
        persistedDocument.VerifiedAt.Should().NotBeNull();
        persistedDocument.VerifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        persistedDocument.OcrData.Should().Contain(verificationNotes);
    }

    [Fact]
    public async Task HandleAsync_WithNonExistentDocument_ShouldThrowNotFoundException()
    {
        // Arrange
        SetupAuthenticatedAdmin();

        var nonExistentId = Guid.NewGuid();
        var command = new ApproveDocumentCommand(nonExistentId, "Notes");

        // Act
        var act = async () => await _handler!.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Document with id {nonExistentId} was not found");
    }

    [Fact]
    public async Task HandleAsync_WithDocumentInWrongStatus_ShouldReturnFailureWithoutPersisting()
    {
        // Arrange
        SetupAuthenticatedAdmin();

        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "documents/identity.pdf");

        // Documento ainda em status Uploaded
        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        var command = new ApproveDocumentCommand(document.Id.Value, "Notes");

        // Act
        var result = await _handler!.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.StatusCode.Should().Be(400);

        // Verificar que o status não mudou no banco
        _dbContext!.ChangeTracker.Clear();
        var persistedDocument = await _repository.GetByIdAsync(document.Id.Value);
        persistedDocument!.Status.Should().Be(EDocumentStatus.Uploaded);
        persistedDocument.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNonAdminUser_ShouldThrowForbiddenAccessException()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new(AuthConstants.Claims.Subject, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "provider")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = principal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "documents/identity.pdf");
        
        document.MarkAsPendingVerification();

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        var command = new ApproveDocumentCommand(document.Id.Value, "Notes");

        // Act
        var act = async () => await _handler!.HandleAsync(command);

        // Assert
        await act.Should().ThrowAsync<ForbiddenAccessException>();

        // Verificar que o documento não foi modificado
        _dbContext!.ChangeTracker.Clear();
        var persistedDocument = await _repository.GetByIdAsync(document.Id.Value);
        persistedDocument!.Status.Should().Be(EDocumentStatus.PendingVerification);
        persistedDocument.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WithNullVerificationNotes_ShouldPersistWithoutNotes()
    {
        // Arrange
        SetupAuthenticatedAdmin();

        var providerId = Guid.NewGuid();
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "identity.pdf",
            "documents/identity.pdf");
        
        document.MarkAsPendingVerification();

        await _repository!.AddAsync(document);
        await _repository.SaveChangesAsync();

        var command = new ApproveDocumentCommand(document.Id.Value, null);

        // Act
        var result = await _handler!.HandleAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _dbContext!.ChangeTracker.Clear();
        var persistedDocument = await _repository.GetByIdAsync(document.Id.Value);

        persistedDocument.Should().NotBeNull();
        persistedDocument!.Status.Should().Be(EDocumentStatus.Verified);
        persistedDocument.OcrData.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ShouldMaintainDataIntegrity_AcrossMultipleApprovals()
    {
        // Arrange
        SetupAuthenticatedAdmin();

        var providerId = Guid.NewGuid();
        
        // Criar múltiplos documentos
        var doc1 = Document.Create(providerId, EDocumentType.IdentityDocument, "id1.pdf", "docs/id1.pdf");
        var doc2 = Document.Create(providerId, EDocumentType.ProofOfResidence, "address1.pdf", "docs/address1.pdf");
        
        doc1.MarkAsPendingVerification();
        doc2.MarkAsPendingVerification();

        await _repository!.AddAsync(doc1);
        await _repository.AddAsync(doc2);
        await _repository.SaveChangesAsync();

        // Act - Aprovar ambos
        var command1 = new ApproveDocumentCommand(doc1.Id.Value, "Doc 1 approved");
        var command2 = new ApproveDocumentCommand(doc2.Id.Value, "Doc 2 approved");

        var result1 = await _handler!.HandleAsync(command1);
        var result2 = await _handler.HandleAsync(command2);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();

        _dbContext!.ChangeTracker.Clear();

        var persistedDoc1 = await _repository.GetByIdAsync(doc1.Id.Value);
        var persistedDoc2 = await _repository.GetByIdAsync(doc2.Id.Value);

        persistedDoc1!.Status.Should().Be(EDocumentStatus.Verified);
        persistedDoc2!.Status.Should().Be(EDocumentStatus.Verified);
        
        persistedDoc1.OcrData.Should().Contain("Doc 1 approved");
        persistedDoc2.OcrData.Should().Contain("Doc 2 approved");
    }
}
