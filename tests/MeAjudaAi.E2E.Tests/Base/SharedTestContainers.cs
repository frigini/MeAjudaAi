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
        if (_initialized) return;

        Console.Error.WriteLine("[DEBUG] SharedTestContainers: EnsureInitializedAsync starting...");
        var diagPath = Path.Combine(AppContext.BaseDirectory, "shared_containers_diag.log");
        try { File.WriteAllText(diagPath, $"[{DateTime.Now}] EnsureInitializedAsync starting...\n"); } catch { }
        
        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        
        await _lock.WaitAsync(cts.Token);
        try
        {
            if (_initialized) return;

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Lock acquired, starting containers...");
            try { File.AppendAllText(diagPath, $"[{DateTime.Now}] Lock acquired, starting containers...\n"); } catch { }
            
            // Configurar switches globais para Npgsql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableGoogleNativeSslStream", true);
            AppContext.SetSwitch("Npgsql.FailOnSslNegotiationFailure", false);

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Building containers...");
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
            
            using var startupCts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            // Start in parallel
            await Task.WhenAll(
                _postgresContainer.StartAsync(startupCts.Token),
                _redisContainer.StartAsync(startupCts.Token),
                _azuriteContainer.StartAsync(startupCts.Token)
            );

            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Containers started successfully.");
            try { File.AppendAllText(diagPath, $"[{DateTime.Now}] Containers started successfully.\n"); } catch { }

            // Gerar connection strings finais com SSL desabilitado explicitamente
            _postgresConnectionString = BuildPostgresConnectionString(_postgresContainer.GetConnectionString());
            _redisConnectionString = _redisContainer.GetConnectionString();
            _azuriteConnectionString = _azuriteContainer.GetConnectionString();

            _initialized = true;
            Console.Error.WriteLine("[DEBUG] SharedTestContainers: Global infrastructure ready.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[FATAL ERROR] SharedTestContainers: Failed to start containers: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
        finally
        {
            _lock.Release();
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
