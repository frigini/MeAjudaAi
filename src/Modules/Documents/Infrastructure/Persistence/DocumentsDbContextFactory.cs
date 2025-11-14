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

        // Try to read from environment first, fallback to default
        // Allows developers to override via: $env:EFCORE_CONNECTION_STRING="Host=remote;..."
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
                              ?? "Host=localhost;Database=meajudaai;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "meajudaai_documents");
            });

        return new DocumentsDbContext(optionsBuilder.Options);
    }
}
