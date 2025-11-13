using MeAjudaAi.Modules.Documents.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using Testcontainers.PostgreSql;

namespace MeAjudaAi.Modules.Documents.Tests.Infrastructure;

public class DocumentsIntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private Respawner _respawner = null!;
    protected IServiceProvider ServiceProvider { get; private set; } = null!;

    protected DocumentsIntegrationTestBase()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("documents_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();
    }

    public async ValueTask InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var services = new ServiceCollection();
        
        services.AddDbContext<DocumentsDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));

        ServiceProvider = services.BuildServiceProvider();

        // Apply migrations
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        await dbContext.Database.MigrateAsync();

        // Initialize Respawner for database cleanup between tests
        await using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,
            SchemasToInclude = new[] { "meajudaai_documents" }
        });
    }

    public async ValueTask DisposeAsync()
    {
        await _postgresContainer.DisposeAsync();
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    protected async Task ResetDatabaseAsync()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
        await using var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    protected DocumentsDbContext GetDbContext()
    {
        var scope = ServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<DocumentsDbContext>();
    }
}
