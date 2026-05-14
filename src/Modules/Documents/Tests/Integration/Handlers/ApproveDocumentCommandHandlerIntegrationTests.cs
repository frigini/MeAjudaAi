using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.Shared.Events;
using MeAjudaAi.Shared.Utilities.Constants;
using MeAjudaAi.Contracts.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Handlers;

[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Application")]
public sealed class ApproveDocumentCommandHandlerIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private DocumentsDbContext? _dbContext;
    private IUnitOfWork? _uow;
    private ApproveDocumentCommandHandler? _handler;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger;

    private readonly Mock<IDomainEventProcessor> _mockDomainEventProcessor;

    public ApproveDocumentCommandHandlerIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder("postgres:15-alpine")
            .WithDatabase("documents_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        _mockLogger = new Mock<ILogger<ApproveDocumentCommandHandler>>();
        _mockDomainEventProcessor = new Mock<IDomainEventProcessor>();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<DocumentsDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new DocumentsDbContext(options, _mockDomainEventProcessor.Object);
        _uow = _dbContext;

        await _dbContext.Database.MigrateAsync();

        _handler = new ApproveDocumentCommandHandler(_uow, _mockHttpContextAccessor.Object, _mockLogger.Object);
    }

    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null) await _dbContext.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task HandleAsync_WithValidDocument_ShouldApprove()
    {
        // Arrange
        var providerId = Guid.NewGuid(); // Explicitly naming providerId for clarity
        var document = Document.Create(
            providerId,
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");
        document.MarkAsPendingVerification();

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var adminId = Guid.NewGuid(); // Admin ID is different from provider ID
        var claims = new List<Claim> { new Claim("sub", adminId.ToString()), new Claim(ClaimTypes.Role, RoleConstants.Admin) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

        var command = new ApproveDocumentCommand(document.Id.Value, "{\"verified\": true}");
        
        // Act
        await _handler!.HandleAsync(command);

        // Assert
        _dbContext!.ChangeTracker.Clear();

        var updatedDocument = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(document.Id);
        updatedDocument.Should().NotBeNull();
        updatedDocument!.Status.Should().Be(EDocumentStatus.Verified);
        updatedDocument.VerifiedAt.Should().NotBeNull();
        updatedDocument.OcrData.Should().Contain("notes").And.Contain("verified");

        // Verify domain events were processed
        _mockDomainEventProcessor.Verify(x => x.ProcessDomainEventsAsync(It.IsAny<IEnumerable<MeAjudaAi.Shared.Events.IDomainEvent>>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_WithNonAdmin_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "url");
        document.MarkAsPendingVerification();
        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var userId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim("sub", userId.ToString()), new Claim(ClaimTypes.Role, RoleConstants.Customer) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(identity) });

        var command = new ApproveDocumentCommand(document.Id.Value, "notes");

        // Act & Assert
        var act = () => _handler!.HandleAsync(command);
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.ForbiddenAccessException>();
    }

    [Fact]
    public async Task HandleAsync_NonExistentDocument_ShouldReturnNotFound()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim("sub", adminId.ToString()), new Claim(ClaimTypes.Role, RoleConstants.Admin) };
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType")) });

        var command = new ApproveDocumentCommand(Guid.NewGuid(), "notes");

        // Act & Assert
        var act = () => _handler!.HandleAsync(command);
        await act.Should().ThrowAsync<MeAjudaAi.Shared.Exceptions.NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_InvalidStatus_ShouldReturnBadRequest()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var document = Document.Create(providerId, EDocumentType.IdentityDocument, "test.pdf", "url");
        // Status is Uploaded (initial), not PendingVerification
        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var adminId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim("sub", adminId.ToString()), new Claim(ClaimTypes.Role, RoleConstants.Admin) };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

        var command = new ApproveDocumentCommand(document.Id.Value, "notes");

        // Act
        var result = await _handler!.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be(ErrorCodes.BadRequest);
    }
}
