using MeAjudaAi.Shared.Database;
using Microsoft.EntityFrameworkCore;

namespace MeAjudaAi.Modules.Locations.Infrastructure.Persistence;

/// <summary>
/// Factory para criação do LocationsDbContext em design time (para migrações)
/// </summary>
/// <remarks>
/// IMPORTANTE: Este pattern é essencial para migrations do EF Core funcionarem corretamente.
/// O namespace `MeAjudaAi.Modules.Locations.Infrastructure.Persistence` permite que
/// a BaseDesignTimeDbContextFactory detecte automaticamente:
/// - Module name: "Locations" (do namespace)
/// - Schema: "locations" (lowercase)
/// - Migrations assembly: "MeAjudaAi.Modules.Locations.Infrastructure"
/// </remarks>
public class LocationsDbContextFactory : BaseDesignTimeDbContextFactory<LocationsDbContext>
{
    protected override string GetDesignTimeConnectionString()
    {
        // Obter de variáveis de ambiente ou user secrets
        // Para configurar: dotnet user-secrets set "ConnectionStrings:Locations" "Host=..."
        // Ou definir variável de ambiente: LOCATIONS_CONNECTION_STRING
        var connectionString = Environment.GetEnvironmentVariable("LOCATIONS_CONNECTION_STRING")
                               ?? Environment.GetEnvironmentVariable("ConnectionStrings__Locations");

        if (!string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        // Construir a partir de componentes individuais
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        // Permitir valores padrão APENAS em ambiente de desenvolvimento local
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        if (isDevelopment)
        {
            // Valores padrão para desenvolvimento local apenas
            host ??= "localhost";
            port ??= "5432";
            database ??= "meajudaai_dev";
            username ??= "postgres";
            password ??= "postgres";

            Console.WriteLine("[WARNING] Using default connection values for Development environment.");
            Console.WriteLine("          Configure environment variables or user secrets for production.");
        }
        else
        {
            // Em ambientes não-dev, EXIGIR configuração explícita
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(password))
            {
                throw new InvalidOperationException(
                    "Missing required database connection configuration for migrations. " +
                    "Set LOCATIONS_CONNECTION_STRING or POSTGRES_HOST/POSTGRES_PASSWORD environment variables.");
            }

            port ??= "5432";
            database ??= "meajudaai";
            username ??= "postgres";
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    protected override string GetMigrationsAssembly()
    {
        return "MeAjudaAi.Modules.Locations.Infrastructure";
    }

    protected override string GetMigrationsHistorySchema()
    {
        return "locations";
    }

    protected override LocationsDbContext CreateDbContextInstance(DbContextOptions<LocationsDbContext> options)
    {
        return new LocationsDbContext(options);
    }
}
