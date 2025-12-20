using DotNet.Testcontainers.Builders;
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
        // Cria container PostgreSQL com PostGIS para suporte a dados geográficos
        // PostGIS é necessário para SearchProviders (NetTopologySuite)
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgis/postgis:15-3.4")
            .WithDatabase("meajudaai_test")
            .WithUsername("postgres")
            .WithPassword("test123")
            .WithPortBinding(0, 5432)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(5432))
            .WithStartupCallback((container, ct) =>
            {
                Console.WriteLine($"[DB-CONTAINER] Started PostgreSQL container {container.Id[..12]} on port {container.GetMappedPublicPort(5432)}");
                return Task.CompletedTask;
            })
            .Build();

        // Cria container Azurite para testes determinísticos de blob storage
        // Fixado na versão 3.33.0 para estabilidade — corresponde ao ambiente de CI/CD de produção
        _azuriteContainer = new AzuriteBuilder()
            .WithImage("mcr.microsoft.com/azure-storage/azurite:3.33.0")
            .WithStartupCallback((container, ct) =>
            {
                Console.WriteLine($"[AZURITE-CONTAINER] Started Azurite container {container.Id[..12]}");
                return Task.CompletedTask;
            })
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
