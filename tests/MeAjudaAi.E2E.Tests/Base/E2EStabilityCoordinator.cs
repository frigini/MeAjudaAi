using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using MeAjudaAi.Shared.Database;
using MeAjudaAi.E2E.Tests.Base.Helpers;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Ratings.Infrastructure.Persistence;
using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using MeAjudaAi.Modules.ServiceCatalogs.Infrastructure.Persistence;
using MeAjudaAi.Modules.Locations.Infrastructure.Persistence;
using MeAjudaAi.Modules.Communications.Infrastructure.Persistence;
using MeAjudaAi.Modules.Bookings.Infrastructure.Persistence;
using MeAjudaAi.Modules.SearchProviders.Infrastructure.Persistence;
using MeAjudaAi.Modules.Payments.Infrastructure.Persistence;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Coordenador centralizado para migrações e limpeza de banco de dados nos testes E2E.
/// Garante que apenas uma thread/processo execute essas operações críticas por vez.
/// </summary>
public static class E2EStabilityCoordinator
{
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static bool _initialized = false;
    private static readonly string _diagPath = Path.Combine(AppContext.BaseDirectory, "stability_coordinator.log");

    public static async Task EnsureInitializedAsync()
    {
        var lockFilePath = Path.Combine(AppContext.BaseDirectory, "e2e_init.lock");
        
        // Usar FileStream com Lock para garantir exclusividade entre processos
        using var lockStream = new FileStream(lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

        if (_initialized) return;

        await _lock.WaitAsync();
        try
        {
            if (_initialized) return;

            await LogAsync("Starting centralized initialization...");

            // 1. Garantir containers ativos
            await SharedTestContainers.EnsureInitializedAsync();
            await LogAsync("Shared containers ready.");

            // 2. Aguardar readiness do Postgres
            await WaitForPostgresAsync();

            // 3. Aplicar Migrações
            await ApplyAllMigrationsAsync();

            // 4. Limpeza inicial
            await GlobalCleanupAsync();

            _initialized = true;
            await LogAsync("Centralized initialization completed successfully.");
        }
        finally
        {
            _lock.Release();
        }
    }

    private static async Task WaitForPostgresAsync()
    {
        await LogAsync("Waiting for Postgres readiness...");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        
        while (!cts.IsCancellationRequested)
        {
            try
            {
                await using var connection = new NpgsqlConnection(SharedTestContainers.PostgresConnectionString);
                await connection.OpenAsync(cts.Token);
                await LogAsync("Postgres is reachable.");
                return;
            }
            catch (Exception)
            {
                await Task.Delay(500, cts.Token);
            }
        }
        
        throw new TimeoutException("Postgres did not become ready within 60 seconds.");
    }

    private static async Task ApplyAllMigrationsAsync()
    {
        await LogAsync("Applying all migrations...");
        
        // Ordem fixa de aplicação para evitar deadlocks de metadados
        // IMPORTANTE: ServiceCatalogs deve vir antes de Providers devido à migração
        // 20260210193620_AddProviderProfileEnhancements que faz backfill usando SQL de service_catalogs.services
        await ApplyIndependentMigration<UsersDbContext>();
        await ApplyIndependentMigration<ServiceCatalogsDbContext>();
        await ApplyIndependentMigration<ProvidersDbContext>();
        await ApplyIndependentMigration<RatingsDbContext>();
        await ApplyIndependentMigration<DocumentsDbContext>();
        await ApplyIndependentMigration<LocationsDbContext>();
        await ApplyIndependentMigration<CommunicationsDbContext>();
        await ApplyIndependentMigration<BookingsDbContext>();
        await ApplyIndependentMigration<SearchProvidersDbContext>();
        await ApplyIndependentMigration<PaymentsDbContext>();
        
        await LogAsync("All migrations applied.");
    }

    private static async Task ApplyIndependentMigration<TContext>() where TContext : DbContext
    {
        var contextName = typeof(TContext).Name;
        await LogAsync($"Migrating {contextName}...");
        
        var optionsBuilder = new DbContextOptionsBuilder<TContext>();
        optionsBuilder.UseNpgsql(SharedTestContainers.PostgresConnectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DbContextSchemaHelper.GetSchemaName(contextName));
            npgsqlOptions.MigrationsAssembly(typeof(TContext).Assembly.FullName);
            
            if (typeof(TContext) == typeof(SearchProvidersDbContext))
            {
                npgsqlOptions.UseNetTopologySuite();
            }
            npgsqlOptions.CommandTimeout(30);
        }).UseSnakeCaseNamingConvention()
        .ConfigureWarnings(warnings => 
            warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));

        using var context = (TContext)Activator.CreateInstance(typeof(TContext), optionsBuilder.Options)!;
        await MigrationTestHelper.ApplyMigrationForContext(context);
    }

    public static async Task GlobalCleanupAsync()
    {
        await LogAsync("Starting global cleanup...");
        
        // Usar um único contexto para listar todas as tabelas de todos os schemas e limpar via SQL puro
        // Isso é mais rápido que limpar contexto por contexto
        await using var connection = new NpgsqlConnection(SharedTestContainers.PostgresConnectionString);
        await connection.OpenAsync();

        // Query para pegar todas as tabelas exceto as de sistema e migração
        var query = @"
            SELECT '""' || table_schema || '"".""' || table_name || '""'
            FROM information_schema.tables 
            WHERE table_schema NOT IN ('information_schema', 'pg_catalog') 
            AND table_name NOT LIKE '__EFMigrationsHistory'";

        var tableNames = new List<string>();
        await using (var cmd = new NpgsqlCommand(query, connection))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                tableNames.Add(reader.GetString(0));
            }
        }

        if (tableNames.Count > 0)
        {
            var truncateSql = $"TRUNCATE TABLE {string.Join(", ", tableNames)} CASCADE";
            await using var truncateCmd = new NpgsqlCommand(truncateSql, connection);
            truncateCmd.CommandTimeout = 15;
            await truncateCmd.ExecuteNonQueryAsync();
            await LogAsync($"Truncated {tableNames.Count} tables.");
        }
        else
        {
            await LogAsync("No tables found to truncate.");
        }
    }

    private static async Task LogAsync(string message)
    {
        var logLine = $"[{DateTime.UtcNow:O}] {message}";
        await File.AppendAllTextAsync(_diagPath, logLine + Environment.NewLine);
        Console.WriteLine(logLine);
    }
}
