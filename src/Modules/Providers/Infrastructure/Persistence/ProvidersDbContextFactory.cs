using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Modules.Providers.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do ProvidersDbContext durante o design-time.
/// Usado pelo Entity Framework para executar migrações.
/// </summary>
public class ProvidersDbContextFactory : IDesignTimeDbContextFactory<ProvidersDbContext>
{
    public ProvidersDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ProvidersDbContext>();

        // Get connection string from secure configuration only
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string 'DefaultConnection' is not configured. " +
                "Please set the connection string in appsettings.json, environment variables, " +
                "or a secure configuration store. For development, ensure appsettings.Development.json " +
                "contains the proper connection string.");
        }

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
        });

        return new ProvidersDbContext(optionsBuilder.Options);
    }
}
