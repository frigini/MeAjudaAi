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
    private PostgreSqlContainer? _postgresContainer;
    private AzuriteContainer? _azuriteContainer;

    /// <summary>
    /// Connection string com detalhes de erro habilitados para diagnóstico em CI
    /// </summary>
    public string? ConnectionString => _postgresContainer != null 
        ? $"{_postgresContainer.GetConnectionString()};Include Error Detail=true" 
        : null;
    
    public string? AzuriteConnectionString => _azuriteContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        // ⚠️ CRÍTICO: Configura Npgsql ANTES de qualquer DbContext ser criado
        // Correção para compatibilidade DateTime UTC com PostgreSQL timestamp
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        Console.WriteLine("[SimpleDatabaseFixture] Npgsql.EnableLegacyTimestampBehavior = true");

        // Cria container PostgreSQL com PostGIS para suporte a dados geográficos
        // PostGIS é necessário para SearchProviders (NetTopologySuite)
        _postgresContainer = new PostgreSqlBuilder("postgis/postgis:16-3.4")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithCleanUp(true)
            .Build();

        // Cria container Azurite para testes determinísticos de blob storage
        // Fixado na versão 3.33.0 para estabilidade — corresponde ao ambiente de CI/CD de produção
        _azuriteContainer = new AzuriteBuilder("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithCleanUp(true)
            .Build();

        // Inicia containers em paralelo para performance
        await Task.WhenAll(
            _postgresContainer.StartAsync(),
            _azuriteContainer.StartAsync()
        );

        // Garante que PostGIS está habilitado (necessário para SearchProviders)
        // Chamado após startup para permitir conexão válida ao banco
        await EnsurePostGisExtensionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Para e descarta containers sequencialmente
        if (_postgresContainer != null)
        {
            Console.WriteLine($"[DB-CONTAINER] Stopping PostgreSQL container {_postgresContainer.Id[..12]}");
            try
            {
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DB-CONTAINER] Error disposing PostgreSQL: {ex.Message}");
            }
        }

        if (_azuriteContainer != null)
        {
            Console.WriteLine($"[AZURITE-CONTAINER] Stopping Azurite container {_azuriteContainer.Id[..12]}");
            try
            {
                await _azuriteContainer.StopAsync();
                await _azuriteContainer.DisposeAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AZURITE-CONTAINER] Error disposing Azurite: {ex.Message}");
            }
        }
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
