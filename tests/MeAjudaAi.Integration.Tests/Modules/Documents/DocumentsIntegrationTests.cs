using FluentAssertions;
using MeAjudaAi.Integration.Tests.Base;
using MeAjudaAi.Modules.Documents.Application.Interfaces;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Modules.Documents;

public class DocumentsIntegrationTests : BaseApiTest
{
    protected override TestModule RequiredModules => TestModule.Documents;

    [Fact]
    public void DocumentRepository_ShouldBeRegisteredInDI()
    {
        using var scope = Services.CreateScope();
        var uow = scope.ServiceProvider.GetService<IUnitOfWork>();

        uow.Should().NotBeNull("IUnitOfWork should be registered");
    }

    [Fact]
    public async Task BlobStorageService_ShouldBeRegisteredInDI()
    {
        await using var scope = Services.CreateAsyncScope();
        var blobService = scope.ServiceProvider.GetService<IBlobStorageService>();

        blobService.Should().NotBeNull("IBlobStorageService should be registered");
    }

    [Fact]
    public void DocumentIntelligenceService_ShouldBeRegisteredInDI()
    {
        using var scope = Services.CreateScope();
        var intelligenceService = scope.ServiceProvider.GetService<IDocumentIntelligenceService>();

        intelligenceService.Should().NotBeNull("IDocumentIntelligenceService should be registered");
    }

    [Fact]
    public void DocumentsDbContext_ShouldBeAbleToConnect()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();

        dbContext.Should().NotBeNull();
    }
}