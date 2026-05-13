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

        await LogAsync("Attempting to acquire lock for EnsureInitializedAsync...");
        await _lock.WaitAsync();
        try
        {
            await LogAsync("Lock acquired for EnsureInitializedAsync.");
            if (_initialized) return;

            await LogAsync("Starting centralized initialization...");

            // 1. Garantir containers ativos
            await SharedTestContainers.EnsureInitializedAsync();
            await LogAsync("Shared containers ready.");

            // 2. Aguardar readiness do Postgres
            await WaitForDatabaseAsync(SharedTestContainers.PostgresConnectionString, TimeSpan.FromSeconds(60), CancellationToken.None);

            // 3. Aplicar Migrações
            await ApplyAllMigrationsAsync();

            // 4. Limpeza inicial
            await InternalGlobalCleanupAsync();

            _initialized = true;
            await LogAsync("Centralized initialization completed successfully.");
        }
        finally
        {
            _lock.Release();
            await LogAsync("Lock released for EnsureInitializedAsync.");
        }
    }

    private static async Task WaitForDatabaseAsync(string cs, TimeSpan maxWait, CancellationToken ct)
    {
        var start = DateTime.UtcNow;
        Exception? last = null;
        await LogAsync("Waiting for database readiness...");
        while (DateTime.UtcNow - start < maxWait && !ct.IsCancellationRequested)
        {
            try
            {
                await using var conn = new NpgsqlConnection(cs + ";Timeout=5;Command Timeout=5;");
                await conn.OpenAsync(ct);
                await conn.CloseAsync();
                await LogAsync("Database is ready.");
                return;
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(500, ct);
            }
        }
        throw new TimeoutException("Database readiness timed out.", last);
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

        var ctor = typeof(TContext).GetConstructor([typeof(DbContextOptions<TContext>)]);
        if (ctor == null)
        {
            throw new InvalidOperationException($"Context {contextName} requires a constructor accepting DbContextOptions<{contextName}>.");
        }

        try
        {
            using var context = (TContext)ctor.Invoke([optionsBuilder.Options]);
            await MigrationTestHelper.ApplyMigrationForContext(context);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to instantiate {contextName} with DbContextOptions.", ex);
        }
    }

    public static async Task GlobalCleanupAsync()
    {
        await LogAsync("Attempting to acquire lock for GlobalCleanupAsync...");
        await _lock.WaitAsync();
        try
        {
            await LogAsync("Lock acquired for GlobalCleanupAsync.");
            await InternalGlobalCleanupAsync();
        }
        finally
        {
            _lock.Release();
            await LogAsync("Lock released for GlobalCleanupAsync.");
        }
    }

    private static async Task InternalGlobalCleanupAsync()
    {
        await LogAsync("Starting global cleanup...");
        try
        {
            var cs = SharedTestContainers.PostgresConnectionString;
            await using var conn = new NpgsqlConnection(cs + ";Timeout=10;Command Timeout=10;");
            await conn.OpenAsync(CancellationToken.None);
            
            await using var setTimeout = conn.CreateCommand();
            setTimeout.CommandText = "SET statement_timeout = '30s';";
            await setTimeout.ExecuteNonQueryAsync();

            await using var list = conn.CreateCommand();
            list.CommandText = @"select format('""%s"".""%s""', table_schema, table_name)
                                 from information_schema.tables
                                 where table_type='BASE TABLE' 
                                 and table_schema IN ('public', 'users', 'providers', 'documents', 'service_catalogs', 'locations', 'communications', 'search_providers', 'ratings', 'payments')";
            
            var names = new List<string>();
            await using (var r = await list.ExecuteReaderAsync())
                while (await r.ReadAsync()) names.Add(r.GetString(0));
            
            if (names.Count == 0)
            {
                await LogAsync("No tables to truncate.");
                return;
            }

            await using var trunc = conn.CreateCommand();
            trunc.CommandText = $"TRUNCATE TABLE {string.Join(", ", names)} CASCADE;";
            await trunc.ExecuteNonQueryAsync();
            await LogAsync($"Truncate completed successfully for {names.Count} tables.");
        }
        catch (Exception ex)
        {
            await LogAsync($"[E2E][WARN] Cleanup skipped: {ex.Message}");
        }
    }
    private static async Task LogAsync(string message)
    {
        var logLine = $"[{DateTime.UtcNow:O}] [COORDINATOR] {message}";
        try 
        { 
            await File.AppendAllTextAsync(_diagPath, logLine + Environment.NewLine); 
        } 
        catch { /* Ignore log failures */ }
        Console.WriteLine(logLine);
    }
}
