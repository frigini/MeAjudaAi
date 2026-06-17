using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do DocumentsDbContext em design time (para migrações)
/// O nome do módulo é detectado automaticamente do namespace
/// </summary>
public class DocumentsDbContextFactory : BaseDesignTimeDbContextFactory<DocumentsDbContext>
{
    protected override DocumentsDbContext CreateDbContextInstance(DbContextOptions<DocumentsDbContext> options)
    {
        return new DocumentsDbContext(options);
    }
}
