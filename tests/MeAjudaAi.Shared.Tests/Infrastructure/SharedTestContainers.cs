using Testcontainers.PostgreSql;
using Microsoft.Extensions.DependencyInjection;
using MeAjudaAi.Shared.Tests.Extensions;

namespace MeAjudaAi.Shared.Tests.Infrastructure;

/// <summary>
/// Container compartilhado para testes de integração de todos os módulos.
/// Reduz overhead de criação/destruição de containers nos testes.
/// Agora usa configurações padronizadas via TestDatabaseOptions.
/// </summary>
public static class SharedTestContainers
{
    private static PostgreSqlContainer? _postgreSqlContainer;
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
        Schema = "public"
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

            // PostgreSQL otimizado para testes com configurações padronizadas
            _postgreSqlContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine") // Imagem menor e mais rápida
                .WithDatabase(_databaseOptions.DatabaseName)
                .WithUsername(_databaseOptions.Username)
                .WithPassword(_databaseOptions.Password)
                .WithPortBinding(0, true) // Porta aleatória para evitar conflitos
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
        
        await _postgreSqlContainer!.StartAsync();
    }

    /// <summary>
    /// Para todos os containers
    /// </summary>
    public static async Task StopAllAsync()
    {
        if (!_isInitialized) return;

        if (_postgreSqlContainer != null)
            await _postgreSqlContainer.StopAsync();
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