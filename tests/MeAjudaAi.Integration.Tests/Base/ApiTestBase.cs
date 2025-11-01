using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.AspNetCore.Authentication;
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
                    // Substitute database with test container - Remove all DbContext related services
                    var usersDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<UsersDbContext>));
                    if (usersDbContextDescriptor != null)
                        services.Remove(usersDbContextDescriptor);

                    var providersDbContextDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ProvidersDbContext>));
                    if (providersDbContextDescriptor != null)
                        services.Remove(providersDbContextDescriptor);

                    // Also remove the actual DbContext services if they exist
                    var usersDbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(UsersDbContext));
                    if (usersDbContextService != null)
                        services.Remove(usersDbContextService);

                    var providersDbContextService = services.SingleOrDefault(d => d.ServiceType == typeof(ProvidersDbContext));
                    if (providersDbContextService != null)
                        services.Remove(providersDbContextService);

                    // Add test database contexts
                    services.AddDbContext<UsersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString);
                        options.EnableSensitiveDataLogging();
                    });

                    services.AddDbContext<ProvidersDbContext>(options =>
                    {
                        options.UseNpgsql(_databaseFixture.ConnectionString);
                        options.EnableSensitiveDataLogging();
                    });

                    // Add test authentication to override any existing authentication
                    services.RemoveRealAuthentication();
                    services.AddConfigurableTestAuthentication();

                    // Remove ClaimsTransformation that causes hanging in tests
                    var claimsTransformationDescriptor = services.FirstOrDefault(d =>
                        d.ServiceType == typeof(IClaimsTransformation));
                    if (claimsTransformationDescriptor != null)
                        services.Remove(claimsTransformationDescriptor);
                });
            });

        Client = _factory.CreateClient();

        // Ensure database schema
        using var scope = _factory.Services.CreateScope();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();
        
        await usersContext.Database.EnsureCreatedAsync();
        await providersContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }
}
