using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MeAjudaAi.Integration.Tests.Base;

/// <summary>
/// Classe base simplificada para testes de integração
/// Cria containers individuais para máxima compatibilidade com CI
/// </summary>
public abstract class ApiTestBase : IAsyncLifetime
{
    private SimpleDatabaseFixture? _databaseFixture;
    private WebApplicationFactory<Program>? _factory;

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _factory!.Services;

    public async ValueTask InitializeAsync()
    {
        _databaseFixture = new SimpleDatabaseFixture();
        await _databaseFixture.InitializeAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    // Substitute database with test container
                    var dbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (dbContextDescriptor != null)
                        services.Remove(dbContextDescriptor);

                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString);
                        options.EnableSensitiveDataLogging();
                    });
                });
            });

        Client = _factory.CreateClient();

        // Ensure database schema
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }
}
