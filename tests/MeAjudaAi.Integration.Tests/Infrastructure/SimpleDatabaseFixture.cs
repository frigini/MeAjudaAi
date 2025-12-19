using DotNet.Testcontainers.Builders;
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

    public string? ConnectionString => _postgresContainer?.GetConnectionString();
    public string? AzuriteConnectionString => _azuriteContainer?.GetConnectionString();

    public async ValueTask InitializeAsync()
    {
        // Cria container PostgreSQL otimizado para CI
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15-alpine")
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
        // Pinned to 3.33.0 for stability - matches production CI/CD environment
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
    }

    public async ValueTask DisposeAsync()
    {
        var disposeTasks = new List<Task>();

        if (_postgresContainer != null)
        {
            Console.WriteLine($"[DB-CONTAINER] Stopping PostgreSQL container {_postgresContainer.Id[..12]}");
            disposeTasks.Add(Task.Run(async () =>
            {
                await _postgresContainer.StopAsync();
                await _postgresContainer.DisposeAsync();
            }));
        }

        if (_azuriteContainer != null)
        {
            Console.WriteLine($"[AZURITE-CONTAINER] Stopping Azurite container {_azuriteContainer.Id[..12]}");
            disposeTasks.Add(Task.Run(async () =>
            {
                await _azuriteContainer.StopAsync();
                await _azuriteContainer.DisposeAsync();
            }));
        }

        await Task.WhenAll(disposeTasks);
    }
}
