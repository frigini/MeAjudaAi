using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;
using DotNet.Testcontainers.Builders;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture que garante que migrations sejam executadas antes dos testes de data seeding.
/// Usa Testcontainers para criar PostgreSQL container automaticamente.
/// Aplica migrations para todos os módulos e executa scripts de seed SQL.
/// </summary>
public sealed class DatabaseMigrationFixture : IAsyncLifetime
{
    private const string SeedsDirectory = "../../../../infrastructure/database/seeds";
    private PostgreSqlContainer? _postgresContainer;

    public string? ConnectionString => _postgresContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        // Cria container PostgreSQL com PostGIS para suporte a dados geográficos
        // PostGIS é necessário para SearchProviders (NetTopologySuite)
        _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithPortBinding(0, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .WithStartupCallback((container, ct) =>
            {
                Console.WriteLine($"[MIGRATION-FIXTURE] Started PostgreSQL container {container.Id[..12]} on port {container.GetMappedPublicPort(5432)}");
                return Task.CompletedTask;
            })
            .Build();

        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();
        
        // Garante que a extensão PostGIS está habilitada (necessária para SearchProviders)
        await EnsurePostGisExtensionAsync(connectionString);
        
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
            
            // Suppress warning about xmin - it's a PostgreSQL system column that doesn't need migration
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
        // Descobre caminho absoluto para os scripts de seed com fallbacks
        var testProjectDir = AppContext.BaseDirectory; // bin/Debug/net10.0
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
            
#pragma warning disable CA2100 // SQL vem de arquivos do projeto, não de input do usuário
            await using var command = new NpgsqlCommand(sql, connection);
#pragma warning restore CA2100
            await command.ExecuteNonQueryAsync();
            
            Console.WriteLine($"[MIGRATION-FIXTURE] Seed executed: {Path.GetFileName(seedFile)}");
        }
    }

    /// <summary>
    /// Garante que a extensão PostGIS está habilitada no banco de dados.
    /// Necessária para SearchProviders (NetTopologySuite/dados geográficos).
    /// </summary>
    private static async Task EnsurePostGisExtensionAsync(string connectionString)
    {
        try
        {
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis;", conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("[MIGRATION-FIXTURE] PostGIS extension verified/created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MIGRATION-FIXTURE] Warning: Could not ensure PostGIS extension: {ex.Message}");
            // Não lança exceção - a imagem postgis/postgis já vem com PostGIS
            // Apenas logamos caso haja algum problema
        }
    }
}
