using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeAjudaAi.Modules.Documents.Infrastructure.Persistence;

/// <summary>
/// Factory para criar DocumentsDbContext em design-time (migrações)
/// </summary>
public class DocumentsDbContextFactory : IDesignTimeDbContextFactory<DocumentsDbContext>
{
    public DocumentsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DocumentsDbContext>();

        // Requer EFCORE_CONNECTION_STRING definida - falha rápida ao invés de usar padrões inseguros
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "EFCORE_CONNECTION_STRING não está definida; defina esta variável de ambiente para configurar a conexão com o banco. "
                + "Exemplo: $env:EFCORE_CONNECTION_STRING=\"Host=localhost;Database=meajudaai;Username=postgres;Password=postgres\"");

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            });

        return new DocumentsDbContext(optionsBuilder.Options);
    }
}
