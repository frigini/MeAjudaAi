using MeAjudaAi.Modules.Documents.Application.Commands;
using MeAjudaAi.Modules.Documents.Application.Handlers.Commands;
using MeAjudaAi.Modules.Documents.Application.Queries.Interfaces;
using MeAjudaAi.Modules.Documents.Domain.Entities;
using MeAjudaAi.Modules.Documents.Domain.Enums;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Tests.Integration.Infrastructure;
using MeAjudaAi.Shared.Database.Abstractions;
using MeAjudaAi.Shared.Resources;
using MeAjudaAi.Shared.Utilities.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace MeAjudaAi.Modules.Documents.Tests.Integration.Handlers;

[Trait("Category", "Integration")]
[Trait("Module", "Documents")]
[Trait("Layer", "Application")]
public sealed class ApproveDocumentCommandHandlerIntegrationTests : DocumentsIntegrationTestBase
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor = new();
    private readonly Mock<ILogger<ApproveDocumentCommandHandler>> _mockLogger = new();
    private readonly Mock<IStringLocalizer<Strings>> _mockLocalizer = new();

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

        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var queries = scope.ServiceProvider.GetRequiredService<IDocumentQueries>();
        var handler = new ApproveDocumentCommandHandler(
            uow,
            queries,
            _mockHttpContextAccessor.Object,
            _mockLogger.Object,
            _mockLocalizer.Object);

        var document = Document.Create(Guid.NewGuid(), EDocumentType.IdentityDocument, "id.pdf", "docs/id.pdf");
        document.MarkAsPendingVerification();

        uow.GetRepository<Document, Guid>().Add(document);
        await uow.SaveChangesAsync();

        var command = new ApproveDocumentCommand(document.Id.Value, "Notes");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        dbContext.ChangeTracker.Clear();
        var persisted = await queries.GetByIdAsync(document.Id.Value);
        persisted!.Status.Should().Be(EDocumentStatus.Verified);
    }
}
