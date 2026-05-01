using FluentAssertions;
using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Domain.ValueObjects;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
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
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning))
            .Options;

        _dbContext = new DocumentsDbContext(options);
        _uow = _dbContext;

        await _dbContext.Database.EnsureCreatedAsync();

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
        var document = Document.Create(
            Guid.NewGuid(),
            EDocumentType.IdentityDocument,
            "test.pdf",
            "blob-url");
        document.MarkAsPendingVerification();

        _uow!.GetRepository<Document, DocumentId>().Add(document);
        await _uow.SaveChangesAsync();

        var adminId = Guid.NewGuid();
        var claims = new List<Claim> { new Claim("sub", adminId.ToString()), new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _mockHttpContextAccessor.Setup(h => h.HttpContext).Returns(new DefaultHttpContext { User = claimsPrincipal });

        var command = new ApproveDocumentCommand(document.Id.Value, "{\"verified\": true}");
        await _handler!.HandleAsync(command);

        var updatedDocument = await _uow.GetRepository<Document, DocumentId>().TryFindAsync(document.Id);
        updatedDocument.Should().NotBeNull();
        updatedDocument!.Status.Should().Be(EDocumentStatus.Verified);
        updatedDocument.VerifiedAt.Should().NotBeNull();
        updatedDocument.OcrData.Should().Be("{\"verified\": true}");
    }
}