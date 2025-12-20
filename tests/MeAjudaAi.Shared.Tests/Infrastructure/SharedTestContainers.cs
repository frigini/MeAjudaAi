using MeAjudaAi.Shared.Tests.Extensions;
using MeAjudaAi.Shared.Tests.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Shared.Tests.Infrastructure;

/// <summary>
/// Container compartilhado para testes de integração de todos os módulos.
/// Reduz overhead de criação/destruição de containers nos testes.
/// Agora usa configurações padronizadas via TestDatabaseOptions.
/// </summary>
public static class SharedTestContainers
{
    private static PostgreSqlContainer? _postgreSqlContainer;
    private static AzuriteContainer? _azuriteContainer;
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
    /// Container Azurite (Azure Storage Emulator) compartilhado para testes de upload/blob storage
    /// </summary>
    public static AzuriteContainer Azurite
    {
        get
        {
            EnsureInitialized();
            return _azuriteContainer!;
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
        Password = "test_password",
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
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgis/postgis:16-3.4") // Mesma versão do CI/CD
                .WithDatabase(_databaseOptions.DatabaseName)
                .WithUsername(_databaseOptions.Username)
                .WithPassword(_databaseOptions.Password)
                .Build();

            // Azurite (Azure Storage Emulator) para testes de blob storage/documents
            // Expõe serviços Blob, Queue e Table
            // Pinned to 3.33.0 for stability - matches production CI/CD environment
            _azuriteContainer = new AzuriteBuilder()
                .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
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
            _azuriteContainer!.StartAsync()
        );

        // Verifica se os containers estão realmente prontos
        await ValidateContainerHealthAsync();
        await ValidateAzuriteHealthAsync();
    }

    /// <summary>
    /// Valida se o container PostgreSQL está saudável e pronto para conexões
    /// </summary>
    private static async Task ValidateContainerHealthAsync()
    {
        if (_postgreSqlContainer == null) return;

        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Tenta obter connection string para verificar se as portas estão mapeadas
                var connectionString = _postgreSqlContainer.GetConnectionString();

                // Se conseguiu obter, o container está pronto
                Console.WriteLine($"Container PostgreSQL ready! Connection: {connectionString}");
                return;
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not mapped"))
            {
                Console.WriteLine($"Container not ready yet (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("PostgreSQL container failed to become ready after maximum retries.");
    }

    /// <summary>
    /// Valida se o container Azurite está saudável e pronto para conexões
    /// </summary>
    private static async Task ValidateAzuriteHealthAsync()
    {
        if (_azuriteContainer == null) return;

        const int maxRetries = 30;
        const int delayMs = 1000;

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                // Tenta verificar se as portas do Azurite estão mapeadas
                var blobPort = _azuriteContainer.GetMappedPublicPort(10000);
                var queuePort = _azuriteContainer.GetMappedPublicPort(10001);
                var tablePort = _azuriteContainer.GetMappedPublicPort(10002);

                // Se conseguiu obter todas as portas, o container está pronto
                Console.WriteLine($"Container Azurite ready! Blob: {blobPort}, Queue: {queuePort}, Table: {tablePort}");
                return;
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Azurite not ready yet (attempt {i + 1}/{maxRetries}): {ex.Message}");
                await Task.Delay(delayMs);
            }
        }

        throw new InvalidOperationException("Azurite container failed to become ready after maximum retries.");
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

        if (_azuriteContainer != null)
            stopTasks.Add(_azuriteContainer.StopAsync());

        await Task.WhenAll(stopTasks);
    }

    /// <summary>
    /// Limpa dados dos containers sem reiniciá-los para um schema específico ou todos
    /// </summary>
    /// <param name="schema">Schema específico para limpar. Se null, usa o schema padrão das configurações</param>
    public static async Task CleanupDataAsync(string? schema = null)
    {
        if (!_isInitialized) return;

        _databaseOptions ??= GetDefaultDatabaseOptions();

        // Limpa PostgreSQL
        if (_postgreSqlContainer != null)
        {
            var schemaToClean = schema ?? _databaseOptions.Schema ?? "public";
            await _postgreSqlContainer.ExecAsync(
            [
                "psql", "-U", _databaseOptions.Username, "-d", _databaseOptions.DatabaseName, "-c",
                $"DROP SCHEMA IF EXISTS {schemaToClean} CASCADE; CREATE SCHEMA {schemaToClean};"
            ]);
        }
    }

    /// <summary>
    /// Limpa todos os schemas de módulos conhecidos
    /// </summary>
    public static async Task CleanupAllModulesAsync()
    {
        if (!_isInitialized) return;

        // Schemas conhecidos dos módulos (pode ser expandido conforme novos módulos)
        var moduleSchemas = new[] { "users", "providers", "services", "orders", "public" };

        foreach (var schema in moduleSchemas)
        {
            await CleanupDataAsync(schema);
        }
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
