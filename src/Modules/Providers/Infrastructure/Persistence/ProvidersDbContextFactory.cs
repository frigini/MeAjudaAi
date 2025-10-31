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

        // Connection string padrão para desenvolvimento/migrações
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Host=localhost;Database=MeAjudaAi;Username=postgres;Password=development123;Search Path=providers,public";

        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
        });

        return new ProvidersDbContext(optionsBuilder.Options);
    }
}