using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture compartilhado para container PostgreSQL
/// Reduz drasticamente o tempo de execução dos testes reutilizando o mesmo container
/// </summary>
public sealed class SharedDatabaseFixture : IAsyncLifetime
{
    private static readonly SemaphoreSlim InitializationSemaphore = new(1, 1);
    private static SharedDatabaseFixture? _instance;
    private static readonly object InstanceLock = new();

    private PostgreSqlContainer? _postgresContainer;
    private bool _isInitialized;

    public static SharedDatabaseFixture Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (InstanceLock)
                {
                    _instance ??= new SharedDatabaseFixture();
                }
            }
            return _instance;
        }
    }

    public string? ConnectionString => _postgresContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        if (_isInitialized)
            return;

        await InitializationSemaphore.WaitAsync();
        try
        {
            if (_isInitialized)
                return;

            // Configura e inicia PostgreSQL uma única vez para todos os testes
            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:15-alpine")
                .WithDatabase("meajudaai_test")
                .WithUsername("postgres")
                .WithPassword("test123")
                .WithPortBinding(0, 5432) // Porta aleatória
                .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
                .WithReuse(true) // IMPORTANTE: Reutiliza container
                .Build();

            await _postgresContainer.StartAsync();
            _isInitialized = true;
        }
        finally
        {
            InitializationSemaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }

    /// <summary>
    /// Limpa dados do banco para isolamento entre testes
    /// Mais rápido que recriar o container
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        if (_postgresContainer == null)
            return;

        var connectionString = _postgresContainer.GetConnectionString();

        // TODO: Implementar limpeza rápida das tabelas se necessário
        // Por enquanto, cada teste deve ser responsável por sua própria limpeza
    }
}
