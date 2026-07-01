using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Queries;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Handlers;

[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Application")]
public sealed class RejectDocumentCommandHandlerIntegrationTests : BaseDatabaseTest
{
    private DocumentsDbContext _dbContext = null!;
    private IUnitOfWork _uow = null!;
    private IDocumentQueries _queries = null!;
    private RejectDocumentCommandHandler _handler = null!;
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<ILogger<RejectDocumentCommandHandler>> _mockLogger = new();
    private readonly Mock<IStringLocalizer<Strings>> _mockLocalizer = new();

    public RejectDocumentCommandHandlerIntegrationTests() : base(schema: Schemas.Documents) { }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();

        var options = CreateDbContextOptions<DocumentsDbContext>();
        _dbContext = new DocumentsDbContext(options);
        await _dbContext.Database.MigrateAsync();
        await InitializeRespawnerAsync();

        _uow = _dbContext;
        _queries = new DbContextDocumentQueries(_dbContext);
        _handler = new RejectDocumentCommandHandler(
            _uow,
            _queries,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockLocalizer.Object);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_dbContext is not null)
            await _dbContext.DisposeAsync();
        await base.DisposeAsync();
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
    public async Task HandleAsync_WithValidDocument_ShouldPersistRejectionToDatabase()
    {
        SetupAuthenticatedAdmin();
        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "id.pdf", "docs/id.pdf");
        document.MarkAsPendingVerification();

        _uow.GetRepository<Document, Guid>().Add(document);
        await _uow.SaveChangesAsync();

        var command = new RejectDocumentCommand(document.Id.Value, "Notes");
        var result = await _handler.HandleAsync(command);

        result.IsSuccess.Should().BeTrue();

        _dbContext.ChangeTracker.Clear();
        var persisted = await _queries.GetByIdAsync(document.Id.Value);
        persisted!.Status.Should().Be(EDocumentStatus.Rejected);
        persisted.RejectionReason.Should().Be("Notes");
    }
}
