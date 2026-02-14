using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Npgsql;
using Testcontainers.Azurite;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture simplificado que cria containers individuais - mais confiável para CI
/// Inclui PostgreSQL e Azurite para testes determinísticos de blob storage
/// </summary>
public sealed class SimpleDatabaseFixture : IAsyncLifetime
{
    private static PostgreSqlContainer? _postgresContainer;
    private static AzuriteContainer? _azuriteContainer;
    
    // Semáforo para garantir inicialização única e thread-safe dos containers
    private static readonly SemaphoreSlim _initializationLock = new(1, 1);
    private static bool _initialized = false;

    /// <summary>
    /// Connection string com detalhes de erro habilitados para diagnóstico em CI
    /// </summary>
    public string? ConnectionString => _postgresContainer != null 
        ? $"{_postgresContainer.GetConnectionString()};Include Error Detail=true" 
        : null;
    
    public string? AzuriteConnectionString => _azuriteContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        // Se já foi inicializado, retorna imediatamente
        if (_initialized) return;

        await _initializationLock.WaitAsync();
        try
        {
            // Verifica novamente dentro do lock (double-check locking pattern)
            if (_initialized) return;

            // ⚠️ CRÍTICO: Configura Npgsql ANTES de qualquer DbContext ser criado
            // Correção para compatibilidade DateTime UTC com PostgreSQL timestamp
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            Console.WriteLine("[SimpleDatabaseFixture] Npgsql.EnableLegacyTimestampBehavior = true");

            // Define e inicia containers apenas se não existirem
            if (_postgresContainer == null)
            {
                // Cria container PostgreSQL com PostGIS para suporte a dados geográficos
                // PostGIS é necessário para SearchProviders (NetTopologySuite)
                _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
                    .WithDatabase("meajudaai_test")
                    .WithUsername("postgres")
                    .WithPassword("test123")
                    .WithCleanUp(true) // Ryuk resource reaper limpará os containers quando o processo terminar
                    .Build();
            }

            if (_azuriteContainer == null)
            {
                // Cria container Azurite para testes determinísticos de blob storage
                // Fixado na versão 3.33.0 para estabilidade — corresponde ao ambiente de CI/CD de produção
                _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
                    .WithCleanUp(true)
                    .Build();
            }

            // Inicia containers em paralelo para performance
            var tasks = new List<Task>();
            
            tasks.Add(_postgresContainer.StartAsync());
            tasks.Add(_azuriteContainer.StartAsync());

            await Task.WhenAll(tasks);

            // Garante que PostGIS está habilitado (necessário para SearchProviders)
            // Chamado após startup para permitir conexão válida ao banco
            await EnsurePostGisExtensionAsync();

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public ValueTask DisposeAsync()
    {
        // Não descartamos os containers aqui porque eles são estáticos e compartilhados.
        // O resource reaper (Ryuk) ou o encerramento do processo cuidará deles.
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Garante que a extensão PostGIS está habilitada no banco de dados.
    /// Necessária para SearchProviders (NetTopologySuite/dados geográficos).
    /// </summary>
    private async Task EnsurePostGisExtensionAsync()
    {
        if (_postgresContainer == null)
            return;

        try
        {
            var connectionString = $"{_postgresContainer.GetConnectionString()};Include Error Detail=true";
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("CREATE EXTENSION IF NOT EXISTS postgis;", conn);
            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("[DB-CONTAINER] PostGIS extension verified/created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DB-CONTAINER] Warning: Could not ensure PostGIS extension: {ex.Message}");
            // Não lança exceção - a imagem postgis/postgis já vem com PostGIS
            // Apenas logamos caso haja algum problema
        }
    }
}

/// <summary>
/// Estratégia de espera customizada para garantir que o PostgreSQL está realmente pronto para aceitar conexões
/// </summary>
internal sealed class WaitUntilDatabaseIsReady : IWaitUntil
{
    public async Task<bool> UntilAsync(IContainer container)
    {
        try
        {
            var connectionString = $"Host=localhost;Port={container.GetMappedPublicPort(5432)};Database=meajudaai_test;Username=postgres;Password=test123;Timeout=5";
            await using var conn = new NpgsqlConnection(connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
