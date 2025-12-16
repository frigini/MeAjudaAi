using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using Npgsql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture que garante que migrations sejam executadas antes dos testes de data seeding.
/// Aplica migrations para todos os módulos e executa scripts de seed SQL.
/// </summary>
public sealed class DatabaseMigrationFixture : IAsyncLifetime
{
    private const string SeedsDirectory = "../../../../infrastructure/database/seeds";

    public async ValueTask InitializeAsync()
    {
        var connectionString = TestConnectionHelper.GetConnectionString();
        
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
        });

        services.AddDbContext<ProvidersDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Providers.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "providers");
            });
            options.UseSnakeCaseNamingConvention();
        });

        services.AddDbContext<DocumentsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Documents.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "documents");
            });
            options.UseSnakeCaseNamingConvention();
        });

        services.AddDbContext<ServiceCatalogsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.ServiceCatalogs.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "service_catalogs");
            });
            options.UseSnakeCaseNamingConvention();
        });

        services.AddDbContext<LocationsDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("MeAjudaAi.Modules.Locations.Infrastructure");
                npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "locations");
            });
            options.UseSnakeCaseNamingConvention();
        });

        await using var serviceProvider = services.BuildServiceProvider();

        // Executa migrations para todos os módulos
        await using (var scope = serviceProvider.CreateAsyncScope())
        {
            var usersDb = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
            await usersDb.Database.MigrateAsync();
            
            var providersDb = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
            await providersDb.Database.MigrateAsync();
            
            var documentsDb = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
            await documentsDb.Database.MigrateAsync();
            
            var serviceCatalogsDb = scope.ServiceProvider.GetRequiredService<ServiceCatalogsDbContext>();
            await serviceCatalogsDb.Database.MigrateAsync();
            
            var locationsDb = scope.ServiceProvider.GetRequiredService<LocationsDbContext>();
            await locationsDb.Database.MigrateAsync();
        }

        // Executa scripts de seed SQL
        await ExecuteSeedScripts(connectionString);
        
        Console.WriteLine("[MIGRATION-FIXTURE] Migrations e seeds executados com sucesso");
    }

    public ValueTask DisposeAsync()
    {
        // Não há recursos para liberar
        return ValueTask.CompletedTask;
    }

    private async Task ExecuteSeedScripts(string connectionString)
    {
        // Descobre caminho absoluto para os scripts de seed com fallbacks
        var currentDir = Directory.GetCurrentDirectory();
        var searchPaths = new[]
        {
            Path.Combine(currentDir, SeedsDirectory),
            Path.Combine(currentDir, "../../../infrastructure/database/seeds"),
            Path.Combine(AppContext.BaseDirectory, SeedsDirectory)
        };
        
        var seedsPath = searchPaths.Select(Path.GetFullPath)
            .FirstOrDefault(Directory.Exists);
        
        if (seedsPath == null)
        {
            var isCI = Environment.GetEnvironmentVariable("CI")?.ToLowerInvariant() is "true" or "1";
            var attemptedPaths = string.Join(", ", searchPaths.Select(Path.GetFullPath));
            
            Console.Error.WriteLine($"[MIGRATION-FIXTURE] Diretório de seeds não encontrado. Tentativas: {attemptedPaths}");
            
            if (isCI)
            {
                throw new DirectoryNotFoundException(
                    $"Seeds directory not found in CI environment. Attempted paths: {attemptedPaths}");
            }
            
            Console.WriteLine("[MIGRATION-FIXTURE] Continuando sem seeds (ambiente de desenvolvimento local)");
            return;
        }

        var seedFiles = Directory.GetFiles(seedsPath, "*.sql").OrderBy(f => f).ToArray();
        
        if (seedFiles.Length == 0)
        {
            Console.WriteLine($"[MIGRATION-FIXTURE] Nenhum arquivo .sql encontrado em {seedsPath}");
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
            
            Console.WriteLine($"[MIGRATION-FIXTURE] Seed executado: {Path.GetFileName(seedFile)}");
        }
    }
}
