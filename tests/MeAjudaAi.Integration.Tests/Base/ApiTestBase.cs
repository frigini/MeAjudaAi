using MeAjudaAi.Integration.Tests.Infrastructure;
using MeAjudaAi.Modules.Providers.Infrastructure.Persistence;
using MeAjudaAi.Modules.Users.Infrastructure.Persistence;
using MeAjudaAi.Shared.Tests.Auth;
using MeAjudaAi.Shared.Tests.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
                
                // Enable detailed logging for debugging
                builder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.SetMinimumLevel(LogLevel.Debug);
                    logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug);
                    logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Debug);
                    logging.AddFilter("MeAjudaAi", LogLevel.Debug);
                });
            });

        Client = _factory.CreateClient();

        // Ensure database schema using EnsureCreatedAsync for testing
        // Note: UsersDbContext has pending model changes that would require new migrations
        using var scope = _factory.Services.CreateScope();
        var usersContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        var providersContext = scope.ServiceProvider.GetRequiredService<ProvidersDbContext>();

        // Create the "providers" schema first (required for ProvidersDbContext)
        await providersContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS providers;");
        
        // For UsersDbContext, use EnsureCreatedAsync (works fine for users)
        await usersContext.Database.EnsureCreatedAsync();
        
        // For ProvidersDbContext, manually create tables since EnsureCreatedAsync doesn't work with custom schema
        try 
        {
            await providersContext.Database.EnsureCreatedAsync();
        }
        catch
        {
            // If EnsureCreatedAsync fails, create tables manually
            await providersContext.Database.ExecuteSqlRawAsync(@"
                CREATE TABLE IF NOT EXISTS providers.providers (
                    id uuid NOT NULL,
                    user_id uuid NOT NULL,
                    name varchar(255) NOT NULL,
                    provider_type integer NOT NULL,
                    created_at timestamp with time zone NOT NULL,
                    updated_at timestamp with time zone,
                    deleted_at timestamp with time zone,
                    CONSTRAINT pk_providers PRIMARY KEY (id)
                );
                
                CREATE TABLE IF NOT EXISTS providers.""Document"" (
                    id uuid NOT NULL,
                    provider_id uuid NOT NULL,
                    document_type integer NOT NULL,
                    document_number varchar(255) NOT NULL,
                    CONSTRAINT pk_document PRIMARY KEY (id),
                    CONSTRAINT fk_document_providers_provider_id FOREIGN KEY (provider_id) REFERENCES providers.providers (id) ON DELETE CASCADE
                );
                
                CREATE TABLE IF NOT EXISTS providers.""Qualification"" (
                    id uuid NOT NULL,
                    provider_id uuid NOT NULL,
                    qualification_type integer NOT NULL,
                    description varchar(1000) NOT NULL,
                    CONSTRAINT pk_qualification PRIMARY KEY (id),
                    CONSTRAINT fk_qualification_providers_provider_id FOREIGN KEY (provider_id) REFERENCES providers.providers (id) ON DELETE CASCADE
                );
            ");
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }
}
