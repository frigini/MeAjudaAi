using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture que garante que migrations sejam executadas antes dos testes de data seeding.
/// Usa SharedTestContainers.CreatePostGisContainer para criar PostgreSQL container próprio.
/// Aplica migrations para todos os módulos e executa scripts de seed SQL.
/// </summary>
public sealed class DatabaseMigrationFixture : IAsyncLifetime
{
    private const string SeedsDirectory = "../../../../infrastructure/database/seeds";
    private PostgreSqlContainer? _postgresContainer;

    public string? ConnectionString => _postgresContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        _postgresContainer = SharedTestContainers.CreatePostGisContainer(
            databaseName: "meajudaai_test",
            username: "postgres",
            password: "test123");

        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        await SharedTestContainers.EnsurePostGisExtensionAsync(connectionString);

        // Cria service provider para ter acesso aos DbContexts
        var services = new ServiceCollection();

        // Registra todos os DbContexts apontando para o mesmo banco de testes
        services.AddDbContext<UsersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Users.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "users");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<ProvidersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<DocumentsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<ServiceCatalogsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "service_catalogs");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        services.AddDbContext<LocationsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Locations.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "locations");
            });
            options.UseSnakeCaseNamingConvention();
            options.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        await using var serviceProvider = services.BuildServiceProvider();

        // Executa migrations para todos os módulos
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            await usersDb.Database.MigrateAsync();

            var serviceCatalogsDb = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            await serviceCatalogsDb.Database.MigrateAsync();

            var providersDb = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            await providersDb.Database.MigrateAsync();

            var documentsDb = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
            await documentsDb.Database.MigrateAsync();

            var locationsDb = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
            await locationsDb.Database.MigrateAsync();
        }

        // Executa scripts de seed SQL
        await ExecuteSeedScripts(connectionString);

        Console.WriteLine("[MIGRATION-FIXTURE] Migrations and seeds executed successfully");
    }

    public async ValueTask DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            Console.WriteLine($"[MIGRATION-FIXTURE] Stopping PostgreSQL container {_postgresContainer.Id[..12]}");
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }

    private async Task ExecuteSeedScripts(string connectionString)
    {
        var testProjectDir = AppContext.BaseDirectory;
        var workspaceRoot = Path.GetFullPath(Path.Combine(testProjectDir, "../../../../../"));

        var searchPaths = new[]
        {
            Path.Combine(workspaceRoot, "infrastructure/database/seeds"),
            Path.Combine(Directory.GetCurrentDirectory(), "infrastructure/database/seeds"),
            Path.Combine(testProjectDir, SeedsDirectory)
        };

        var seedsPath = searchPaths.Select(Path.GetFullPath)
            .FirstOrDefault(Directory.Exists);

        if (seedsPath == null)
        {
            var isCI = Environment.GetEnvironmentVariable("CI")?.ToLowerInvariant() is "true" or "1";
            var attemptedPaths = string.Join(", ", searchPaths.Select(Path.GetFullPath));

            Console.Error.WriteLine($"[MIGRATION-FIXTURE] Seeds directory not found. Attempted paths: {attemptedPaths}");

            if (isCI)
            {
                throw new DirectoryNotFoundException(
                    $"Seeds directory not found in CI environment. Attempted paths: {attemptedPaths}");
            }

            Console.WriteLine("[MIGRATION-FIXTURE] Continuing without seeds (local development environment)");
            return;
        }

        var seedFiles = Directory.GetFiles(seedsPath, "*.sql").OrderBy(f => f).ToArray();

        if (seedFiles.Length == 0)
        {
            Console.WriteLine($"[MIGRATION-FIXTURE] No .sql files found in {seedsPath}");
            return;
        }

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        foreach (var seedFile in seedFiles)
        {
            var sql = await File.ReadAllTextAsync(seedFile);

            await using var command = new NpgsqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();

            Console.WriteLine($"[MIGRATION-FIXTURE] Seed executed: {Path.GetFileName(seedFile)}");
        }
    }
}
