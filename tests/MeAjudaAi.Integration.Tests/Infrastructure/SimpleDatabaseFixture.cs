using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Integration.Tests.Infrastructure;

/// <summary>
/// Fixture simplificado que cria containers individuais - mais confiável para CI
/// </summary>
public sealed class SimpleDatabaseFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _postgresContainer;

    public string? ConnectionString => _postgresContainer?.GetConnectionString();

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

        await _postgresContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_postgresContainer != null)
        {
            Console.WriteLine($"[DB-CONTAINER] Stopping PostgreSQL container {_postgresContainer.Id[..12]}");
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
    }
}
