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
        if (_initialized) return;

        var lockFilePath = Path.Combine(AppContext.BaseDirectory, "e2e_init.lock");
        const int maxRetries = 120; // 2 minutos de espera total
        
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
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
                    return;
                }
                finally
                {
                    _lock.Release();
                }
            }
            catch (IOException)
            {
                // Arquivo travado por outro processo, aguardar e tentar novamente
                if (i == maxRetries - 1) 
                    throw new TimeoutException($"Could not acquire E2E initialization lock after {maxRetries} attempts.");
                
                await Task.Delay(1000);
            }
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

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        await using var connection = new NpgsqlConnection(SharedTestContainers.PostgresConnectionString);
        await connection.OpenAsync(cts.Token);

        try
        {
            // Set short statement timeout for TRUNCATE
            await using (var timeoutCmd = new NpgsqlCommand("SET statement_timeout = '30s'", connection))
            {
                await timeoutCmd.ExecuteNonQueryAsync(cts.Token);
            }

            // Query para pegar todas as tabelas base exceto as de sistema, migração e extensões (PostGIS)
            var query = @"
                SELECT '""' || table_schema || '"".""' || table_name || '""'
                FROM information_schema.tables 
                WHERE table_type = 'BASE TABLE'
                AND table_schema NOT IN ('information_schema', 'pg_catalog') 
                AND table_name NOT LIKE '__EFMigrationsHistory'
                AND table_name NOT IN ('spatial_ref_sys')";

            var tableNames = new List<string>();
            await using (var cmd = new NpgsqlCommand(query, connection))
            await using (var reader = await cmd.ExecuteReaderAsync(cts.Token))
            {
                while (await reader.ReadAsync(cts.Token))
                {
                    tableNames.Add(reader.GetString(0));
                }
            }

            if (tableNames.Count > 0)
            {
                await LogAsync($"Truncating {tableNames.Count} tables...");
                var truncateSql = $"TRUNCATE TABLE {string.Join(", ", tableNames)} CASCADE";
                await using var truncateCmd = new NpgsqlCommand(truncateSql, connection);
                await truncateCmd.ExecuteNonQueryAsync(cts.Token);
                await LogAsync("Truncate completed successfully.");
            }
            else
            {
                await LogAsync("No tables found to truncate.");
            }
        }
        catch (Exception ex)
        {
            await LogAsync($"Cleanup FAILED: {ex.Message}");
            throw;
        }
    }
    private static async Task LogAsync(string message)
    {
        var logLine = $"[{DateTime.UtcNow:O}] {message}";
        await File.AppendAllTextAsync(_diagPath, logLine + Environment.NewLine);
        Console.WriteLine(logLine);
    }
}
