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
        static SharedTestContainers()
        {
            AppDomain.CurrentDomain.ProcessExit += (_, _) =>
            {
                try { DisposeAsync().GetAwaiter().GetResult(); } catch { }
            };
        }

    private static PostgreSqlContainer? _postgresContainer;
    private static RedisContainer? _redisContainer;
    private static AzuriteContainer? _azuriteContainer;
    private static readonly SemaphoreSlim _lock = new(1, 1);
    private static bool _initialized = false;

    private static string _postgresConnectionString = string.Empty;
    private static string _redisConnectionString = string.Empty;
    private static string _azuriteConnectionString = string.Empty;

    public static string PostgresConnectionString
    {
        get
        {
            if (!_initialized)
                throw new InvalidOperationException("EnsureInitializedAsync must be called before accessing PostgresConnectionString");
            return _postgresConnectionString;
        }
    }

    public static string RedisConnectionString
    {
        get
        {
            if (!_initialized)
                throw new InvalidOperationException("EnsureInitializedAsync must be called before accessing RedisConnectionString");
            return _redisConnectionString;
        }
    }

    public static string AzuriteConnectionString
    {
        get
        {
            if (!_initialized)
                throw new InvalidOperationException("EnsureInitializedAsync must be called before accessing AzuriteConnectionString");
            return _azuriteConnectionString;
        }
    }

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

            var dbUser = Environment.GetEnvironmentVariable("TEST_DB_USER") ?? "postgres";
            var dbPassword = Environment.GetEnvironmentVariable("TEST_DB_PASSWORD") ?? Guid.NewGuid().ToString("N")[..16];
            Console.Error.WriteLine($"[DEBUG] SharedTestContainers: Using DB user={dbUser}, password from env or generated.");

            _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
                .WithDatabase("meajudaai_test")
                .WithUsername(dbUser)
                .WithPassword(dbPassword)
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

            var startedContainers = new List<(string Name, Task StartTask)>();
            Exception? startupException = null;

            try
            {
                startedContainers.Add(("postgres", _postgresContainer.StartAsync(startupCts.Token)));
                startedContainers.Add(("redis", _redisContainer.StartAsync(startupCts.Token)));
                startedContainers.Add(("azurite", _azuriteContainer.StartAsync(startupCts.Token)));

                await Task.WhenAll(startedContainers.Select(c => c.StartTask));
            }
            catch (Exception ex)
            {
                startupException = ex;
                AppendDiag($"[ERROR] Container startup failed: {ex.Message}. Cleaning up started containers...");
                Console.Error.WriteLine($"[ERROR] Container startup failed: {ex.Message}. Cleaning up started containers...");
            }

            if (startupException != null)
            {
                foreach (var (name, startTask) in startedContainers)
                {
                    try
                    {
                        if (startTask.IsCompletedSuccessfully || startTask.Status == TaskStatus.RanToCompletion)
                        {
                            AppendDiag($"Stopping container {name} due to startup failure...");
                            switch (name)
                            {
                                case "postgres":
                                    await _postgresContainer.StopAsync(CancellationToken.None);
                                    break;
                                case "redis":
                                    await _redisContainer.StopAsync(CancellationToken.None);
                                    break;
                                case "azurite":
                                    await _azuriteContainer.StopAsync(CancellationToken.None);
                                    break;
                            }
                        }
                    }
                    catch (Exception cleanupEx)
                    {
                        AppendDiag($"[WARN] Failed to stop container {name}: {cleanupEx.Message}");
                        Console.Error.WriteLine($"[WARN] Failed to stop container {name}: {cleanupEx.Message}");
                    }
                }

                throw startupException;
            }

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
            try
            {
                await _redisContainer.ExecAsync(new[] { "redis-cli", "FLUSHALL" });
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[WARN] FlushRedisAsync failed: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    /// <summary>
    /// Resets the internal state for testing purposes.
    /// NOTE: This method does NOT dispose or stop the Docker containers.
    /// The caller is responsible for proper teardown if needed.
    /// Containers will continue running and will be reused on next EnsureInitializedAsync call.
    /// </summary>
    public static void ResetForTesting()
    {
        _initialized = false;
        _postgresConnectionString = string.Empty;
        _redisConnectionString = string.Empty;
        _azuriteConnectionString = string.Empty;
        _postgresContainer = null;
        _redisContainer = null;
        _azuriteContainer = null;
    }

    public static bool IsInitialized => _initialized;

    public static string GetStatus()
    {
        return $"[_initialized={_initialized}, postgres={_postgresContainer != null}, redis={_redisContainer != null}, azurite={_azuriteContainer != null}]";
    }

    /// <summary>
    /// Disposes all containers and releases resources.
    /// Safe to call multiple times - subsequent calls are no-ops.
    /// </summary>
    public static async Task DisposeAsync()
    {
        if (!_initialized && _postgresContainer == null && _redisContainer == null && _azuriteContainer == null)
        {
            return;
        }

        var containers = new List<(string Name, IAsyncDisposable? Container)>
        {
            ("postgres", _postgresContainer),
            ("redis", _redisContainer),
            ("azurite", _azuriteContainer)
        };

        foreach (var (name, container) in containers)
        {
            if (container != null)
            {
                try
                {
                    await container.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"[WARN] Failed to dispose container {name}: {ex.Message}");
                }
            }
        }

        _postgresContainer = null;
        _redisContainer = null;
        _azuriteContainer = null;
        _initialized = false;

        try
        {
            _lock.Dispose();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[WARN] Failed to dispose lock: {ex.Message}");
        }
    }
}
