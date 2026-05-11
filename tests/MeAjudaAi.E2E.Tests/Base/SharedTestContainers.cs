using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;
using Testcontainers.Azurite;
using Npgsql;

namespace MeAjudaAi.E2E.Tests.Base;

/// <summary>
/// Singleton centralizado para gerenciar containers de teste compartilhados em todo o assembly.
/// Evita conflitos entre TestContainerFixture e BaseTestContainerTest.
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

        var diagPath = Path.Combine(AppContext.BaseDirectory, "shared_containers_diag.log");
        File.WriteAllText(diagPath, $"[{DateTime.Now}] EnsureInitializedAsync starting...\n");
        await _lock.WaitAsync();
        try
        {
            if (_initialized) return;

            File.AppendAllText(diagPath, $"[{DateTime.Now}] Lock acquired, starting containers...\n");
            Console.WriteLine("🚀 [SharedTestContainers] Starting global infrastructure...");
            
            // Configurar switches globais para Npgsql
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            AppContext.SetSwitch("Npgsql.DisableGoogleNativeSslStream", true);
            AppContext.SetSwitch("Npgsql.FailOnSslNegotiationFailure", false);

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

            // Start in parallel
            await Task.WhenAll(
                _postgresContainer.StartAsync(),
                _redisContainer.StartAsync(),
                _azuriteContainer.StartAsync()
            );

            File.AppendAllText(diagPath, $"[{DateTime.Now}] Containers started successfully.\n");

            // Gerar connection strings finais com SSL desabilitado explicitamente
            _postgresConnectionString = BuildPostgresConnectionString(_postgresContainer.GetConnectionString());
            _redisConnectionString = _redisContainer.GetConnectionString();
            _azuriteConnectionString = _azuriteContainer.GetConnectionString();

            _initialized = true;
            Console.WriteLine("✅ [SharedTestContainers] Global infrastructure ready.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ [SharedTestContainers] Failed to start containers: {ex.Message}");
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
