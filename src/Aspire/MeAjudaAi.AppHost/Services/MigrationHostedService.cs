using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace MeAjudaAi.AppHost.Services;

/// <summary>
/// Hosted service que roda migrations na inicialização do AppHost
/// </summary>
internal class MigrationHostedService(
    ILogger<MigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("🔄 Starting migrations for all modules...");

        List<Type> dbContextTypes = new();

        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
            
            // Pular migrations em ambientes de teste - são gerenciadas pela infraestrutura de testes
            if (environment.Equals("Testing", StringComparison.OrdinalIgnoreCase) || 
                environment.Equals("Test", StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("⏭️ Skipping migrations in {Environment} environment", environment);
                return;
            }
            
            var isDevelopment = environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

            var connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                if (isDevelopment)
                {
                    logger.LogWarning("⚠️ Connection string not found in Development, skipping migrations");
                    return;
                }
                else
                {
                    logger.LogError("❌ Connection string is required for migrations in {Environment} environment. " +
                        "Configure POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, and POSTGRES_PASSWORD.", environment);
                    throw new InvalidOperationException(
                        $"Database connection configuration missing for {environment} environment. " +
                        "Migrations cannot proceed without a valid connection string.");
                }
            }

            dbContextTypes = DiscoverDbContextTypes();
            logger.LogInformation("📋 Found {Count} DbContexts for migration", dbContextTypes.Count);

            foreach (var contextType in dbContextTypes)
            {
                await MigrateDbContextAsync(contextType, connectionString, cancellationToken);
            }

            logger.LogInformation("✅ All migrations applied successfully!");
            
            // Executar seeding após migrations
            await ExecuteSeedingAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error applying migrations for {DbContextCount} module(s)", dbContextTypes.Count);
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
            // Valores padrão APENAS para desenvolvimento local
            // Use arquivo .env ou user secrets para a senha
            host ??= "localhost";
            port ??= "5432";
            database ??= "meajudaai";
            username ??= "postgres";
            // Senha é obrigatória mesmo em dev - use variável de ambiente
            if (string.IsNullOrEmpty(password))
            {
                logger.LogWarning(
                    "POSTGRES_PASSWORD not configured for Development environment. " +
                    "Set the environment variable or use user secrets.");
                return null;
            }

            logger.LogWarning(
                "Using default connection values for Development environment. " +
                "Configure environment variables for production deployments.");
        }
        else
        {
            // Em ambientes não-dev, EXIGIR configuração explícita
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(port) ||
                string.IsNullOrEmpty(database) || string.IsNullOrEmpty(username) ||
                string.IsNullOrEmpty(password))
            {
                logger.LogError(
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

        // Primeiro, tentar carregar assemblies de módulos dinamicamente
        LoadModuleAssemblies();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .ToList();

        if (assemblies.Count == 0)
        {
            logger.LogWarning("⚠️ No module assemblies found. Migrations will not be applied automatically.");
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
                    logger.LogDebug("✅ Discovered {Count} DbContext(s) in {Assembly}", types.Count, assembly.GetName().Name);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "⚠️ Error discovering types in assembly {AssemblyName}", assembly.FullName);
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

            logger.LogDebug("🔍 Searching for module assemblies in: {BaseDirectory}", baseDirectory);
            logger.LogDebug("📦 Found {Count} module infrastructure DLLs", moduleDlls.Length);

            foreach (var dllPath in moduleDlls)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dllPath);

                    // Verificar se já foi carregado
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assemblyName.FullName))
                    {
                        logger.LogDebug("⏭️  Assembly already loaded: {AssemblyName}", assemblyName.Name);
                        continue;
                    }

                    System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                    logger.LogDebug("✅ Assembly loaded: {AssemblyName}", assemblyName.Name);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "⚠️ Could not load assembly: {DllPath}", Path.GetFileName(dllPath));
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "⚠️ Error attempting to dynamically load module assemblies");
        }
    }

    private async Task MigrateDbContextAsync(Type contextType, string connectionString, CancellationToken cancellationToken)
    {
        var moduleName = ExtractModuleName(contextType);
        logger.LogInformation("🔧 Applying migrations for {Module}...", moduleName);

        try
        {
            // Criar DbContextOptions diretamente via reflexão para manter type safety
            var assemblyName = contextType.Assembly.FullName
                ?? contextType.Assembly.GetName().Name
                ?? contextType.Assembly.ToString();

            // Usar DbContextOptionsBuilder<TContext> genérico para garantir type safety
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder = Activator.CreateInstance(optionsBuilderType)
                ?? throw new InvalidOperationException($"Failed to create DbContextOptionsBuilder for {contextType.Name}");
            
            // Configurar PostgreSQL usando método de extensão UseNpgsql
            var useNpgsqlMethod = typeof(NpgsqlDbContextOptionsBuilderExtensions)
                .GetMethods()
                .FirstOrDefault(m => 
                    m.Name == "UseNpgsql" && 
                    m.GetParameters().Length == 3 &&
                    m.GetParameters()[1].ParameterType == typeof(string));
            
            if (useNpgsqlMethod == null)
                throw new InvalidOperationException("UseNpgsql extension method not found");
            
            Action<Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder> npgsqlOptionsAction = npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(assemblyName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            };
            
            useNpgsqlMethod.Invoke(null, new object[] { optionsBuilder, connectionString, npgsqlOptionsAction });

            // Acessar Options usando reflexão para manter o tipo genérico DbContextOptions<TContext>
            var optionsProperty = optionsBuilderType.GetProperty("Options")
                ?? throw new InvalidOperationException($"Options property not found on {optionsBuilderType.Name}");
            var options = optionsProperty.GetValue(optionsBuilder)
                ?? throw new InvalidOperationException($"Failed to get Options from DbContextOptionsBuilder for {contextType.Name}");

            // NOTA: Todos os DbContexts devem ter um construtor público aceitando DbContextOptions<TContext>.
            // Esta é uma restrição de design aplicada em toda a codebase.
            if (Activator.CreateInstance(contextType, options) is not DbContext dbContext)
            {
                throw new InvalidOperationException(
                    $"Failed to create DbContext instance of type {contextType.Name}. " +
                    "Ensure the DbContext has a public constructor that accepts DbContextOptions.");
            }

            using (dbContext)
            {
                // Aplicar migrations
                var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("📦 {Module}: {Count} pending migrations", moduleName, pendingMigrations.Count);
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogDebug("   - {Migration}", migration);
                    }

                    await dbContext.Database.MigrateAsync(cancellationToken);
                    logger.LogInformation("✅ {Module}: Migrations applied successfully", moduleName);
                }
                else
                {
                    logger.LogInformation("✓ {Module}: No pending migrations", moduleName);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Error applying migrations for {Module}", moduleName);
            throw new InvalidOperationException(
                $"Failed to apply database migrations for module '{moduleName}' (DbContext: {contextType.Name})",
                ex);
        }
    }

    private static string ExtractModuleName(Type contextType)
    {
        // Extrair nome do módulo do namespace (ex: MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext -> Users)
        var namespaceParts = contextType.Namespace?.Split('.') ?? Array.Empty<string>();
        var moduleIndex = Array.IndexOf(namespaceParts, "Modules");

        if (moduleIndex >= 0 && moduleIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[moduleIndex + 1];
        }

        return contextType.Name.Replace("DbContext", "");
    }

    private async Task ExecuteSeedingAsync(CancellationToken cancellationToken)
    {
        // Executar seeding apenas em ambiente Development
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
        if (!environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogInformation("⏭️ Skipping data seeding in {Environment} environment", environment);
            return;
        }

        try
        {
            logger.LogInformation("🌱 Executing data seeding for Development environment...");
            
            var connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogWarning("⚠️ Cannot execute seeding: connection string not available");
                return;
            }

            // Executar scripts SQL de seeding
            await using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Encontrar e executar scripts de seed em infrastructure/database/seeds/
            var seedScriptsPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "infrastructure", "database", "seeds");
            var normalizedPath = Path.GetFullPath(seedScriptsPath);
            
            if (Directory.Exists(normalizedPath))
            {
                var seedFiles = Directory.GetFiles(normalizedPath, "*.sql").OrderBy(f => f).ToList();
                
                foreach (var seedFile in seedFiles)
                {
                    var fileName = Path.GetFileName(seedFile);
                    logger.LogInformation("📜 Executing seed script: {FileName}", fileName);
                    
                    var sqlScript = await File.ReadAllTextAsync(seedFile, cancellationToken);
                    await using var command = new Npgsql.NpgsqlCommand(sqlScript, connection);
                    command.CommandTimeout = 120; // 2 minutos de timeout para scripts de seed
                    
                    await command.ExecuteNonQueryAsync(cancellationToken);
                    logger.LogInformation("✅ Seed script executed: {FileName}", fileName);
                }
                
                logger.LogInformation("✅ Data seeding completed successfully! ({Count} scripts)", seedFiles.Count);
            }
            else
            {
                logger.LogWarning("⚠️ Seed scripts directory not found: {Path}", normalizedPath);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Respeitar o cancelamento e permitir que ele propague
            throw;
        }
        catch (Npgsql.NpgsqlException ex)
        {
            logger.LogError(ex, "❌ Database error during data seeding");
            // Ignorar intencionalmente para evitar falha no startup de dev; ajuste se desejar falhar em erros de banco de dados.
        }
        catch (IOException ex)
        {
            logger.LogError(ex, "❌ IO error while reading seed scripts");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "❌ Access denied while reading seed scripts");
        }
    }
}

