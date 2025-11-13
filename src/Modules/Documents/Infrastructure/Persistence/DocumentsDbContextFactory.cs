using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Factory para criar DocumentsDbContext em design-time (migrations)
/// </summary>
public class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DocumentsDbContext>();
        
        // Connection string padrão para migrations
        // Em produção, isso virá da configuração
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=meajudaai;Username=postgres;Password=postgres",
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__ef_migrations_history", "meajudaai_documents");
            });

        return new DocumentsDbContext(optionsBuilder.Options);
    }
}
