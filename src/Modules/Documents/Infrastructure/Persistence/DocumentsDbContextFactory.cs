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

        // Require EFCORE_CONNECTION_STRING to be set - fail fast instead of using insecure defaults
        var connectionString = Environment.GetEnvironmentVariable("EFCORE_CONNECTION_STRING")
            ?? throw new InvalidOperationException(
                "EFCORE_CONNECTION_STRING is not set; set this environment variable to configure the database connection. "
                + "Example: $env:EFCORE_CONNECTION_STRING=\"Host=localhost;Database=meajudaai;Username=postgres;Password=postgres\"");

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            });

        return new DocumentsDbContext(optionsBuilder.Options);
    }
}
