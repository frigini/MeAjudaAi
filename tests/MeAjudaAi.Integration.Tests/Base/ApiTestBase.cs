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

        // For ProvidersDbContext, use migrations for proper table structure
        try
        {
            logger?.LogInformation("🔄 Running Providers migrations...");
            await providersContext.Database.MigrateAsync();
            logger?.LogInformation("✅ Providers database migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "⚠️ Migrations failed for Providers, trying EnsureCreatedAsync");

            try
            {
                await providersContext.Database.EnsureCreatedAsync();
                logger?.LogInformation("✅ Providers database schema created with EnsureCreatedAsync");
            }
            catch (Exception ensureEx)
            {
                logger?.LogWarning(ensureEx, "⚠️ EnsureCreatedAsync also failed, falling back to manual creation");

                try
                {
                    await CreateProvidersTableManually(providersContext, logger);
                    logger?.LogInformation("✅ Providers database schema created using manual table creation");
                }
                catch (Exception manualEx)
                {
                    logger?.LogError(manualEx, "❌ All Providers table creation methods failed");
                    throw new InvalidOperationException("Unable to initialize Providers database schema", manualEx);
                }
            }
        }

        // Verify tables exist
        try
        {
            var usersCount = await usersContext.Users.CountAsync();
            logger?.LogInformation("Users database verification successful - Count: {UsersCount}", usersCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Users database verification failed");
            throw new InvalidOperationException("Users database is not properly initialized", ex);
        }

        try
        {
            var providersCount = await providersContext.Providers.CountAsync();
            logger?.LogInformation("Providers database verification successful - Count: {ProvidersCount}", providersCount);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Providers database verification failed - attempting emergency table creation");

            // Emergency table creation as last resort
            try
            {
                await CreateProvidersTableManually(providersContext, logger);

                // Retry verification after manual creation
                var providersCount = await providersContext.Providers.CountAsync();
                logger?.LogInformation("Emergency table creation successful - Count: {ProvidersCount}", providersCount);
            }
            catch (Exception emergencyEx)
            {
                logger?.LogError(emergencyEx, "Emergency table creation also failed");
                throw new InvalidOperationException("Providers database could not be initialized despite all attempts", emergencyEx);
            }
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
        logger?.LogInformation("🔨 Starting manual Providers table creation with clean slate");

        // First, drop existing tables to ensure clean slate
        await context.Database.ExecuteSqlRawAsync(@"
            DROP TABLE IF EXISTS providers.qualification CASCADE;
            DROP TABLE IF EXISTS providers.document CASCADE;
            DROP TABLE IF EXISTS providers.providers CASCADE;
        ");

        // Create the main providers table with all necessary columns based on the EF Core model
        var createProvidersTable = @"
            CREATE TABLE IF NOT EXISTS providers.providers (
                id uuid PRIMARY KEY,
                user_id uuid NOT NULL,
                name varchar(100) NOT NULL,
                type varchar(20) NOT NULL,
                verification_status varchar(20) NOT NULL,
                is_deleted boolean NOT NULL DEFAULT false,
                deleted_at timestamp with time zone,
                created_at timestamp with time zone NOT NULL DEFAULT NOW(),
                updated_at timestamp with time zone,
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

        // Create the documents table (owned entity)
        var createDocumentsTable = @"
            CREATE TABLE IF NOT EXISTS providers.document (
                provider_id uuid NOT NULL,
                id serial PRIMARY KEY,
                number varchar(50) NOT NULL,
                document_type varchar(20) NOT NULL,
                is_primary boolean NOT NULL DEFAULT false,
                FOREIGN KEY (provider_id) REFERENCES providers.providers(id) ON DELETE CASCADE
            );";

        // Create the qualifications table (owned entity)  
        var createQualificationsTable = @"
            CREATE TABLE IF NOT EXISTS providers.qualification (
                provider_id uuid NOT NULL,
                id serial PRIMARY KEY,
                name varchar(200) NOT NULL,
                description varchar(1000),
                issuing_organization varchar(200),
                issue_date timestamp with time zone,
                expiration_date timestamp with time zone,
                document_number varchar(50),
                FOREIGN KEY (provider_id) REFERENCES providers.providers(id) ON DELETE CASCADE
            );";

        var createIndices = @"
            CREATE UNIQUE INDEX IF NOT EXISTS ix_providers_user_id ON providers.providers (user_id);
            CREATE INDEX IF NOT EXISTS ix_providers_name ON providers.providers (name);
            CREATE INDEX IF NOT EXISTS ix_providers_type ON providers.providers (type);
            CREATE INDEX IF NOT EXISTS ix_providers_verification_status ON providers.providers (verification_status);
            CREATE INDEX IF NOT EXISTS ix_providers_is_deleted ON providers.providers (is_deleted);
            CREATE UNIQUE INDEX IF NOT EXISTS ix_document_provider_id_document_type ON providers.document (provider_id, document_type);";

        try
        {
            await context.Database.ExecuteSqlRawAsync(createProvidersTable);
            logger?.LogInformation("✅ Created providers table manually");

            await context.Database.ExecuteSqlRawAsync(createDocumentsTable);
            logger?.LogInformation("✅ Created documents table manually");

            await context.Database.ExecuteSqlRawAsync(createQualificationsTable);
            logger?.LogInformation("✅ Created qualifications table manually");

            await context.Database.ExecuteSqlRawAsync(createIndices);
            logger?.LogInformation("✅ Created indices manually");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Failed to create tables manually");
            throw;
        }
    }
}
