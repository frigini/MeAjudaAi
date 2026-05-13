using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Testcontainers.Azurite;
using Npgsql;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Singleton centralizado para gerenciar containers de teste compartilhados em todo o assembly.
/// Garante que todos os testes E2E usem a mesma infraestrutura base.
/// </summary>
public static class SharedTestContainers
{
    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static AzuriteContainer? _azuriteContainer;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static bool _initialized = false;

    private static string _postgresConnectionString = string.Empty;
    private static string _redisConnectionString = string.Empty;
    private static string _azuriteConnectionString = string.Empty;

    public static string PostgresConnectionString => _postgresConnectionString;
    public static string RedisConnectionString => _redisConnectionString;
    public static string AzuriteConnectionString => _azuriteConnectionString;

    public static async Task EnsureInitializedAsync()
    {
        var diagPath = Path.Combine(AppContext.BaseDirectory, "shared_containers_diag.log");
        void AppendDiag(string msg) { try { File.AppendAllText(diagPath, $"[{DateTime.Now:O}] {msg}\n"); } catch { } }

        if (_initialized)
        {
            AppendDiag("Already initialized, returning immediately.");
            return;
        }

        Console.Error.WriteLine("[DEBUG] SharedTestContainers: EnsureInitializedAsync starting...");
        try { File.WriteAllText(diagPath, $"[{DateTime.Now}] EnsureInitializedAsync starting...\n"); } catch { }

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));

        if (!_lock.Wait(0, cts.Token))
        {
            AppendDiag("Could not acquire lock immediately, waiting...");
            await _lock.WaitAsync(cts.Token);
        }
        try
        {
            if (_initialized)
            {
                AppendDiag("Double-check: already initialized after acquiring lock.");
                return;
            }

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Lock acquired, starting containers...");
            try { File.AppendAllText(diagPath, $"[{DateTime.Now}] Lock acquired, starting containers...\n"); } catch { }

            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableGoogleNativeSslStream", true);
            AppContext.SetSwitch("Npgsql.FailOnSslNegotiationFailure", false);

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Building containers...");
            AppendDiag("Building containers...");
            _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("meajudaai_test")
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithCleanUp(true)
                .Build();

            _redisContainer = new RedisBuilder("redis:7-alpine")
                .WithCleanUp(true)
                .Build();

            _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
                .WithCleanUp(true)
                .Build();

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Starting containers in parallel (Timeout: 4m)...");
            AppendDiag("Starting containers...");
            using var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            await Task.WhenAll(
                _postgresContainer.StartAsync(startupCts.Token),
                _redisContainer.StartAsync(startupCts.Token),
                _azuriteContainer.StartAsync(startupCts.Token)
            );

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Containers started successfully.");
            try { File.AppendAllText(diagPath, $"[{DateTime.Now}] Containers started successfully.\n"); } catch { }

            _postgresConnectionString = BuildPostgresConnectionString(_postgresContainer.GetConnectionString());
            _redisConnectionString = _redisContainer.GetConnectionString();
            _azuriteConnectionString = _azuriteContainer.GetConnectionString();

            _initialized = true;
            AppendDiag("Set _initialized = true. Infrastructure ready.");
            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Global infrastructure ready.");
        }
        catch (Exception ex)
        {
            AppendDiag($"[FATAL ERROR] SharedTestContainers: Failed: {ex.Message}\n{ex.StackTrace}");
            Console.Error.WriteLine($"[FATAL ERROR] SharedTestContainers: Failed to start containers: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
        finally
        {
            _lock.Release();
            AppendDiag("Lock released.");
        }
    }

    private static string BuildPostgresConnectionString(string raw)
    {
        var builder = new NpgsqlConnectionStringBuilder(raw)
        {
            SslMode = SslMode.Disable,
            IncludeErrorDetail = true,
            Timeout = 30,
            CommandTimeout = 60,
            Pooling = false // Desabilita pooling nos testes E2E para evitar locks residuais durante cleanup
        };
        return builder.ToString();
    }

    public static async Task FlushRedisAsync()
    {
        if (_redisContainer != null)
        {
            await _redisContainer.ExecAsync(new[] { "redis-cli", "FLUSHALL" });
        }
    }
}
