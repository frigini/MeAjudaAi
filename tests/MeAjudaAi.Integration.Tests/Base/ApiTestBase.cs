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
        // Set environment variables for testing
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Testing");

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
        var logger = scope.ServiceProvider.GetService<ILogger<ApiTestBase>>();

        // Create schemas first
        try
        {
            await providersContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS providers;");
            await usersContext.Database.ExecuteSqlRawAsync("CREATE SCHEMA IF NOT EXISTS users;");
            logger?.LogInformation("Database schemas created successfully");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Failed to create schemas, they may already exist");
        }

        // For UsersDbContext, use EnsureCreatedAsync (works fine for users)
        try
        {
            await usersContext.Database.EnsureCreatedAsync();
            logger?.LogInformation("Users database schema created successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to create Users database schema");
            throw;
        }

        // For ProvidersDbContext, use a more robust approach
        try
        {
            await providersContext.Database.EnsureCreatedAsync();
            logger?.LogInformation("Providers database schema created successfully with EnsureCreatedAsync");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "EnsureCreatedAsync failed for Providers, attempting alternative approach");

            try
            {
                // Generate and execute the creation script manually
                var createScript = providersContext.Database.GenerateCreateScript();
                
                // Split script by semicolons and execute each statement separately
                var statements = createScript
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .Where(s => !s.StartsWith("--") && !string.IsNullOrEmpty(s));

                foreach (var statement in statements)
                {
                    try
                    {
                        await providersContext.Database.ExecuteSqlRawAsync(statement + ";");
                    }
                    catch (Exception sqlEx)
                    {
                        // Log but continue - some statements might fail if objects already exist
                        logger?.LogWarning(sqlEx, "Failed to execute SQL statement: {Statement}", statement.Substring(0, Math.Min(100, statement.Length)));
                    }
                }

                logger?.LogInformation("Providers database schema created using manual script execution");
            }
            catch (Exception scriptEx)
            {
                logger?.LogError(scriptEx, "Failed to create Providers database schema with manual script");
                
                // Last resort: Create the basic table structure manually
                try
                {
                    await CreateProvidersTableManually(providersContext, logger);
                    logger?.LogInformation("Providers database schema created using manual table creation");
                }
                catch (Exception manualEx)
                {
                    logger?.LogError(manualEx, "All attempts to create Providers database schema failed");
                    throw;
                }
            }
        }

        // Verify tables exist
        try
        {
            var usersCount = await usersContext.Users.CountAsync();
            var providersCount = await providersContext.Providers.CountAsync();
            logger?.LogInformation("Database verification successful - Users: {UsersCount}, Providers: {ProvidersCount}", usersCount, providersCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Database verification failed");
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        _factory?.Dispose();
        if (_databaseFixture != null)
            await _databaseFixture.DisposeAsync();
    }

    private static async Task CreateProvidersTableManually(ProvidersDbContext context, ILogger? logger)
    {
        // Create the main providers table with all necessary columns
        var createProvidersTable = @"
            CREATE TABLE IF NOT EXISTS providers.providers (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                name varchar(100) NOT NULL,
                type varchar(20) NOT NULL,
                verification_status varchar(20) NOT NULL,
                is_deleted boolean NOT NULL DEFAULT false,
                deleted_at timestamp with time zone,
                legal_name varchar(200) NOT NULL,
                fantasy_name varchar(200),
                description varchar(1000),
                email varchar(255) NOT NULL,
                phone_number varchar(20),
                website varchar(255),
                street varchar(200) NOT NULL,
                number varchar(20) NOT NULL,
                complement varchar(100),
                neighborhood varchar(100) NOT NULL,
                city varchar(100) NOT NULL,
                state varchar(50) NOT NULL,
                zip_code varchar(20) NOT NULL,
                country varchar(50) NOT NULL
            );";

        var createIndices = @"
            CREATE UNIQUE INDEX IF NOT EXISTS ix_providers_user_id ON providers.providers (user_id);
            CREATE INDEX IF NOT EXISTS ix_providers_name ON providers.providers (name);
            CREATE INDEX IF NOT EXISTS ix_providers_type ON providers.providers (type);
            CREATE INDEX IF NOT EXISTS ix_providers_verification_status ON providers.providers (verification_status);
            CREATE INDEX IF NOT EXISTS ix_providers_is_deleted ON providers.providers (is_deleted);";

        await context.Database.ExecuteSqlRawAsync(createProvidersTable);
        await context.Database.ExecuteSqlRawAsync(createIndices);
        
        logger?.LogInformation("Created providers table and indices manually");
    }
}
