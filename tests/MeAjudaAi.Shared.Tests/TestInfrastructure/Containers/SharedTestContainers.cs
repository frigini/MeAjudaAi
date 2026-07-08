using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Helpers;
using MeAjudaAi.Shared.Tests.TestInfrastructure.Options;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace MeAjudaAi.Shared.Tests.TestInfrastructure.Containers;

/// <summary>
/// Container compartilhado para testes de integração de todos os módulos.
/// Reduz overhead de criação/destruição de containers nos testes.
/// Agora usa configurações padronizadas via TestDatabaseOptions.
/// </summary>
public static class SharedTestContainers
{
    private static PostgreSqlContainer? _postgreSqlContainer;
    private static RabbitMqContainer? _rabbitMqContainer;
    private static TestDatabaseOptions? _databaseOptions;
    private static readonly Lock _lock = new();
    private static bool _isInitialized;

    /// <summary>
    /// Container PostgreSQL compartilhado para todos os testes
    /// </summary>
    public static PostgreSqlContainer PostgreSql
    {
        get
        {
            EnsureInitialized();
            return _postgreSqlContainer!;
        }
    }

    /// <summary>
    /// Container RabbitMQ compartilhado para testes de mensageria
    /// </summary>
    public static RabbitMqContainer RabbitMq
    {
        get
        {
            EnsureInitialized();
            return _rabbitMqContainer!;
        }
    }

    /// <summary>
    /// Inicializa com configurações específicas (usado pelos testes)
    /// </summary>
    public static void Initialize(TestDatabaseOptions? databaseOptions = null)
    {
        _databaseOptions = databaseOptions ?? GetDefaultDatabaseOptions();
        EnsureInitialized();
    }

    /// <summary>
    /// Configurações padrão do banco para testes compartilhados
    /// </summary>
    private static TestDatabaseOptions GetDefaultDatabaseOptions() => new()
    {
        DatabaseName = "test_db",
        Username = "test_user",
        Password = "test123",
        Schema = "users" // Usado como padrão para garantir compatibilidade com UsersModule migrations
    };

    /// <summary>
    /// Inicializa os containers compartilhados (thread-safe)
    /// </summary>
    private static void EnsureInitialized()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            _databaseOptions ??= GetDefaultDatabaseOptions();

            // PostgreSQL com PostGIS para suportar queries geoespaciais nos testes
            // Usando postgis/postgis:16-3.4 (mesma versão do CI/CD para garantir consistência)
            _postgreSqlContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4") // Mesma versão do CI/CD
                .WithDatabase(_databaseOptions.DatabaseName)
                .WithUsername(_databaseOptions.Username)
                .WithPassword(_databaseOptions.Password)
                .Build();

            // RabbitMQ para testes de mensageria
            _rabbitMqContainer = new RabbitMqBuilder("rabbitmq:4.0.9-management")
                .WithUsername("guest")
                .WithPassword("guest")
                .Build();

            _isInitialized = true;
        }
    }

    /// <summary>
    /// Inicia todos os containers de forma assíncrona
    /// </summary>
    public static async Task StartAllAsync()
    {
        EnsureInitialized();

        // Inicia containers em paralelo para performance
        await Task.WhenAll(
            _postgreSqlContainer!.StartAsync(),
            _rabbitMqContainer!.StartAsync()
        );

        // Verifica se os containers estão realmente prontos
        await Task.WhenAll(
            ValidateContainerHealthAsync(),
            ValidateRabbitMqHealthAsync()
        );
    }

    /// <summary>
    /// Valida se o container PostgreSQL está saudável e pronto para conexões
    /// </summary>
    private static async Task ValidateContainerHealthAsync()
    {
        if (_postgreSqlContainer == null)
        {
            throw new InvalidOperationException("ValidateContainerHealthAsync: _postgreSqlContainer is null. Ensure EnsureInitialized() has been called before health checks.");
        }

        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                var connectionString = _postgreSqlContainer.GetConnectionString();
                Console.WriteLine($"[PostgreSQL] Attempt {i + 1}/{maxRetries}: connecting...");

                await using (var connection = new Npgsql.NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                }

                Console.WriteLine($"[PostgreSQL] Container ready!");
                return;
            }
            catch (Npgsql.PostgresException ex) when (ex.SqlState == "3D000")
            {
                Console.WriteLine($"[PostgreSQL] Database not ready (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not mapped"))
            {
                Console.WriteLine($"[PostgreSQL] Container not ready (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PostgreSQL] Health check error (attempt {i + 1}/{maxRetries}): {ex.GetType().Name}: {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("PostgreSQL container failed to become ready after maximum retries.");
    }

    /// <summary>
    /// Para todos os containers
    /// </summary>
    public static async Task StopAllAsync()
    {
        if (!_isInitialized) return;

            var stopTasks = new List<Task>();

            if (_postgreSqlContainer != null)
                stopTasks.Add(_postgreSqlContainer.StopAsync());

            if (_rabbitMqContainer != null)
                stopTasks.Add(_rabbitMqContainer.StopAsync());

        await Task.WhenAll(stopTasks);
    }

    /// <summary>
    /// Limpa dados dos containers sem reiniciá-los para um schema específico ou todos
    /// </summary>
    /// <param name="schema">Schema específico para limpar. Se null, usa o schema padrão das configurações</param>
    public static async Task CleanupDataAsync(string? schema = null)
    {
        if (!_isInitialized || _postgreSqlContainer == null) return;

        // Verifica se o container foi iniciado antes de tentar executar comandos
        try
        {
            _ = _postgreSqlContainer.Id;
        }
        catch (InvalidOperationException)
        {
            // Container não foi iniciado, nada a limpar
            return;
        }

        _databaseOptions ??= GetDefaultDatabaseOptions();

        var schemaToClean = schema ?? _databaseOptions.Schema ?? "public";

        var result = await _postgreSqlContainer.ExecAsync(
        [
            "psql", "-U", _databaseOptions.Username, "-d", _databaseOptions.DatabaseName, "-tAc",
            $"""
            DO $$
            DECLARE
                r RECORD;
            BEGIN
                FOR r IN SELECT tablename FROM pg_tables WHERE schemaname = '{schemaToClean}' AND tablename <> '__EFMigrationsHistory'
                LOOP
                    EXECUTE format('TRUNCATE TABLE %I.%I CASCADE', '{schemaToClean}', r.tablename);
                END LOOP;
            END $$;
            """
        ]);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"CleanupDataAsync failed for schema '{schemaToClean}' with exit code {result.ExitCode}: {result.Stdout}{result.Stderr}");
        }
    }

    /// <summary>
    /// Valida se o container RabbitMQ está saudável e pronto para conexões
    /// </summary>
    private static async Task ValidateRabbitMqHealthAsync()
    {
        if (_rabbitMqContainer == null) return;

        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Tenta obter a connection string e validá-la
                var connectionString = _rabbitMqContainer.GetConnectionString();
                
                // Tenta fazer uma conexão básica com a string obtida
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // A string deve ser válida para AMQP
                    var uri = new Uri(connectionString);
                    if (uri.Scheme == "amqp" || uri.Scheme == "amqps")
                    {
                        Console.WriteLine($"RabbitMQ container ready! Endpoint: {uri.Scheme}://{uri.Host}:{uri.Port}");
                        return;
                    }
                }
                
                Console.WriteLine($"RabbitMQ not ready yet - invalid connection string (attempt {i + 1}/{maxRetries})");
                await Task.Delay(delayMs);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"RabbitMQ not ready yet (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RabbitMQ validation error (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("RabbitMQ container failed to become ready after maximum retries.");
    }

    /// <summary>
    /// Limpa todos os schemas de módulos conhecidos
    /// </summary>
    public static async Task CleanupAllModulesAsync()
    {
        if (!_isInitialized) return;

        foreach (var schema in DbContextSchemaHelper.GetAllModuleSchemas())
        {
            await CleanupDataAsync(schema);
        }

        await CleanupDataAsync("public");
    }

    /// <summary>
    /// Aplica automaticamente todas as migrações descobertas para o service provider fornecido.
    /// Este método é chamado durante a inicialização dos testes de integração.
    /// </summary>
    /// <param name="serviceProvider">Service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    public static async Task ApplyAllMigrationsAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized) return;

        // Log dos DbContexts descobertos para debug
        var discoveredContexts = MigrationDiscoveryExtensions.GetDiscoveredDbContextNames();
        Console.WriteLine($"Auto-discovered DbContexts: {string.Join(", ", discoveredContexts)}");

        // Aplica todas as migrações descobertas automaticamente
        await serviceProvider.ApplyAllDiscoveredMigrationsAsync(cancellationToken);
    }

    /// <summary>
    /// Inicializa o banco de dados com todas as migrações descobertas automaticamente.
    /// Este método combina criação do banco e aplicação de migrações.
    /// </summary>
    /// <param name="serviceProvider">Service provider contendo os DbContexts registrados</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    public static async Task InitializeDatabaseWithMigrationsAsync(
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        if (!_isInitialized) return;

        // Garante que todos os bancos de dados são criados e migrados
        await serviceProvider.EnsureAllDatabasesCreatedAsync(cancellationToken);
    }
}
