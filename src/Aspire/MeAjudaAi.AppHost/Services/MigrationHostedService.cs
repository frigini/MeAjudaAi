using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MeAjudaAi.AppHost.Services;

/// <summary>
/// Hosted service que roda migrations na inicialização do AppHost
/// </summary>
internal class MigrationHostedService : IHostedService
{
    private readonly ILogger<MigrationHostedService> _logger;

    public MigrationHostedService(
        ILogger<MigrationHostedService> logger)
    {
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Starting migrations for all modules...");

        List<Type> dbContextTypes = new();

        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            
            // Skip migrations in test environments - they are managed by test infrastructure
            if (environment.Equals("Testing", StringComparison.OrdinalIgnoreCase) || 
                environment.Equals("Test", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("⏭️ Skipping migrations in {Environment} environment", environment);
                return;
            }
            
            var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

            var connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                if (isDevelopment)
                {
                    _logger.LogWarning("⚠️ Connection string not found in Development, skipping migrations");
                    return;
                }
                else
                {
                    _logger.LogError("❌ Connection string is required for migrations in {Environment} environment. " +
                        "Configure POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, and POSTGRES_PASSWORD.", environment);
                    throw new InvalidOperationException(
                        $"Database connection configuration missing for {environment} environment. " +
                        "Migrations cannot proceed without a valid connection string.");
                }
            }

            dbContextTypes = DiscoverDbContextTypes();
            _logger.LogInformation("📋 Found {Count} DbContexts for migration", dbContextTypes.Count);

            foreach (var contextType in dbContextTypes)
            {
                await MigrateDbContextAsync(contextType, connectionString, cancellationToken);
            }

            _logger.LogInformation("✅ All migrations applied successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error applying migrations for {DbContextCount} module(s)", dbContextTypes.Count);
            throw new InvalidOperationException(
                $"Failed to apply database migrations for {dbContextTypes.Count} module(s)",
                ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private string? GetConnectionString()
    {
        // Obter de variáveis de ambiente (padrão Aspire)
        var host = Environment.GetEnvironmentVariable("POSTGRES_HOST")
                   ?? Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT")
                   ?? Environment.GetEnvironmentVariable("DB_PORT");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB")
                       ?? Environment.GetEnvironmentVariable("MAIN_DATABASE");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER")
                       ?? Environment.GetEnvironmentVariable("DB_USERNAME");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
                       ?? Environment.GetEnvironmentVariable("DB_PASSWORD");

        // Para ambiente de desenvolvimento local apenas, permitir valores padrão
        // NUNCA use valores padrão em produção - configure variáveis de ambiente adequadamente
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        if (isDevelopment)
        {
            // Default values ONLY for local development
            // Use .env file or user secrets for password
            host ??= "localhost";
            port ??= "5432";
            database ??= "meajudaai";
            username ??= "postgres";
            // Password is required even in dev - use environment variable
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning(
                    "POSTGRES_PASSWORD not configured for Development environment. " +
                    "Set the environment variable or use user secrets.");
                return null;
            }

            _logger.LogWarning(
                "Using default connection values for Development environment. " +
                "Configure environment variables for production deployments.");
        }
        else
        {
            // In non-dev environments, REQUIRE explicit configuration
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
                string.IsNullOrEmpty(database) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                _logger.LogError(
                    "Database connection configuration missing. " +
                    "Set the environment variables POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, and POSTGRES_PASSWORD.");
                return null; // Fail startup to prevent insecure connection
            }
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Timeout=30;Command Timeout=60";
    }

    private List<Type> DiscoverDbContextTypes()
    {
        var dbContextTypes = new List<Type>();

        // First, try to dynamically load module assemblies
        LoadModuleAssemblies();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .ToList();

        if (assemblies.Count == 0)
        {
            _logger.LogWarning("⚠️ No module assemblies found. Migrations will not be applied automatically.");
            return dbContextTypes;
        }

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
                    .Where(t => t.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(types);

                if (types.Count > 0)
                {
                    _logger.LogDebug("✅ Discovered {Count} DbContext(s) in {Assembly}", types.Count, assembly.GetName().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Error discovering types in assembly {AssemblyName}", assembly.FullName);
            }
        }

        return dbContextTypes;
    }

    private void LoadModuleAssemblies()
    {
        try
        {
            var baseDirectory = AppContext.BaseDirectory;
            var modulePattern = "MeAjudaAi.Modules.*.Infrastructure.dll";
            var moduleDlls = Directory.GetFiles(baseDirectory, modulePattern, SearchOption.AllDirectories);

            _logger.LogDebug("🔍 Searching for module assemblies in: {BaseDirectory}", baseDirectory);
            _logger.LogDebug("📦 Found {Count} module infrastructure DLLs", moduleDlls.Length);

            foreach (var dllPath in moduleDlls)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dllPath);

                    // Check if already loaded
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assemblyName.FullName))
                    {
                        _logger.LogDebug("⏭️  Assembly already loaded: {AssemblyName}", assemblyName.Name);
                        continue;
                    }

                    System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                    _logger.LogDebug("✅ Assembly loaded: {AssemblyName}", assemblyName.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Could not load assembly: {DllPath}", Path.GetFileName(dllPath));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error attempting to dynamically load module assemblies");
        }
    }

    private async Task MigrateDbContextAsync(Type contextType, string connectionString, CancellationToken cancellationToken)
    {
        var moduleName = ExtractModuleName(contextType);
        _logger.LogInformation("🔧 Applying migrations for {Module}...", moduleName);

        try
        {
            // Create DbContextOptions directly without reflection
            var assemblyName = contextType.Assembly.FullName
                ?? contextType.Assembly.GetName().Name
                ?? contextType.Assembly.ToString();

            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(assemblyName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            var options = optionsBuilder.Options;

            if (options == null)
            {
                throw new InvalidOperationException($"Could not create DbContextOptions for {contextType.Name}");
            }

            // Criar instância do DbContext usando o construtor
            var dbContext = Activator.CreateInstance(contextType, options) as DbContext;

            if (dbContext == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create DbContext instance of type {contextType.Name}. " +
                    "Ensure the DbContext has a public constructor that accepts DbContextOptions.");
            }

            using (dbContext)
            {
                // Apply migrations
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("📦 {Module}: {Count} pending migrations", moduleName, pendingMigrations.Count);
                    foreach (var migration in pendingMigrations)
                    {
                        _logger.LogDebug("   - {Migration}", migration);
                    }

                    await dbContext.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("✅ {Module}: Migrations applied successfully", moduleName);
                }
                else
                {
                    _logger.LogInformation("✓ {Module}: No pending migrations", moduleName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error applying migrations for {Module}", moduleName);
            throw new InvalidOperationException(
                $"Failed to apply database migrations for module '{moduleName}' (DbContext: {contextType.Name})",
                ex);
        }
    }

    private static string ExtractModuleName(Type contextType)
    {
        // Extract module name from namespace (e.g., MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext -> Users)
        var namespaceParts = contextType.Namespace?.Split('.') ?? Array.Empty<string>();
        var moduleIndex = Array.IndexOf(namespaceParts, "Modules");

        if (moduleIndex >= 0 && moduleIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[moduleIndex + 1];
        }

        return contextType.Name.Replace("DbContext", "");
    }
}

