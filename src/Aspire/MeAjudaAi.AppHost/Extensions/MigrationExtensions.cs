using System.Reflection;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MeAjudaAi.AppHost.Extensions;

/// <summary>
/// Extensões para aplicar migrations automaticamente no Aspire
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Adiciona e executa migrations de todos os módulos antes de iniciar a aplicação
    /// </summary>
    public static IResourceBuilder<T> WithMigrations<T>(
        this IResourceBuilder<T> builder) where T : IResourceWithConnectionString
    {
        builder.ApplicationBuilder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IHostedService, MigrationHostedService>());
        return builder;
    }
}

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
        _logger.LogInformation("🔄 Iniciando migrations de todos os módulos...");

        List<Type> dbContextTypes = new();

        try
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";
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
                        $"Missing database connection configuration for {environment} environment. " +
                        "Migrations cannot proceed without valid connection string.");
                }
            }

            dbContextTypes = DiscoverDbContextTypes();
            _logger.LogInformation("📋 Encontrados {Count} DbContexts para migração", dbContextTypes.Count);

            foreach (var contextType in dbContextTypes)
            {
                await MigrateDbContextAsync(contextType, connectionString, cancellationToken);
            }

            _logger.LogInformation("✅ Todas as migrations foram aplicadas com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao aplicar migrations para {DbContextCount} módulo(s)", dbContextTypes.Count);
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
            // TODO: Considerar usar .env file ou user secrets para valores de dev
            host ??= "localhost";
            port ??= "5432";
            database ??= "meajudaai";
            username ??= "postgres";
            password ??= "test123"; // Somente dev local - NUNCA em produção!

            _logger.LogWarning(
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
                _logger.LogError(
                    "Missing required database connection configuration. " +
                    "Set POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DB, POSTGRES_USER, and POSTGRES_PASSWORD " +
                    "environment variables.");
                return null; // Falhar startup para evitar conexão insegura
            }
        }

        return $"Host={host};Port={port};Database={database};Username={username};Password={password};Timeout=30;Command Timeout=60";
    }

    private List<Type> DiscoverDbContextTypes()
    {
        var dbContextTypes = new List<Type>();

        // Primeiro, tentar carregar assemblies dos módulos dinamicamente
        LoadModuleAssemblies();

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .ToList();

        if (assemblies.Count == 0)
        {
            _logger.LogWarning("⚠️ Nenhum assembly de módulo foi encontrado. Migrations não serão aplicadas automaticamente.");
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
                    _logger.LogDebug("✅ Descobertos {Count} DbContext(s) em {Assembly}", types.Count, assembly.GetName().Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Erro ao descobrir tipos no assembly {AssemblyName}", assembly.FullName);
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

            _logger.LogDebug("🔍 Procurando por assemblies de módulos em: {BaseDirectory}", baseDirectory);
            _logger.LogDebug("📦 Encontrados {Count} DLLs de infraestrutura de módulos", moduleDlls.Length);

            foreach (var dllPath in moduleDlls)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dllPath);

                    // Verificar se já está carregado
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName == assemblyName.FullName))
                    {
                        _logger.LogDebug("⏭️  Assembly já carregado: {AssemblyName}", assemblyName.Name);
                        continue;
                    }

                    System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
                    _logger.LogDebug("✅ Assembly carregado: {AssemblyName}", assemblyName.Name);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Não foi possível carregar assembly: {DllPath}", Path.GetFileName(dllPath));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Erro ao tentar carregar assemblies de módulos dinamicamente");
        }
    }

    private async Task MigrateDbContextAsync(Type contextType, string connectionString, CancellationToken cancellationToken)
    {
        var moduleName = ExtractModuleName(contextType);
        _logger.LogInformation("🔧 Aplicando migrations para {Module}...", moduleName);

        try
        {
            // Criar DbContextOptionsBuilder dinâmicamente mantendo tipo genérico
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilderInstance = Activator.CreateInstance(optionsBuilderType);

            if (optionsBuilderInstance == null)
            {
                throw new InvalidOperationException($"Não foi possível criar DbContextOptionsBuilder para {contextType.Name}");
            }

            // Configurar PostgreSQL - usar dynamic para simplificar reflexão
            dynamic optionsBuilderDynamic = optionsBuilderInstance;

            // Safe assembly name: FullName can be null for some assemblies
            var assemblyName = contextType.Assembly.FullName
                ?? contextType.Assembly.GetName().Name
                ?? contextType.Assembly.ToString();

            // Chamar UseNpgsql com connection string
            Microsoft.EntityFrameworkCore.NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql(
                optionsBuilderDynamic,
                connectionString,
                (Action<Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.NpgsqlDbContextOptionsBuilder>)(npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsAssembly(assemblyName);
                    npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                })
            );

            // Obter Options com tipo correto via reflection
            var optionsProperty = optionsBuilderType.GetProperty("Options");
            if (optionsProperty == null)
            {
                throw new InvalidOperationException(
                    $"Could not find 'Options' property on DbContextOptionsBuilder<{contextType.Name}>. " +
                    "This indicates a version mismatch or reflection issue.");
            }

            var options = optionsProperty.GetValue(optionsBuilderInstance);
            if (options == null)
            {
                throw new InvalidOperationException(
                    $"DbContextOptions for {contextType.Name} is null after configuration. " +
                    "Ensure UseNpgsql was called successfully.");
            }

            // Verify constructor exists before attempting instantiation
            var constructor = contextType.GetConstructor(new[] { options.GetType() });
            if (constructor == null)
            {
                throw new InvalidOperationException(
                    $"No suitable constructor found for {contextType.Name} that accepts {options.GetType().Name}. " +
                    "Ensure the DbContext has a constructor that accepts DbContextOptions.");
            }

            // Criar instância do DbContext
            var contextInstance = Activator.CreateInstance(contextType, options);
            var context = contextInstance as DbContext;

            if (context == null)
            {
                throw new InvalidOperationException(
                    $"Failed to cast created instance to DbContext for type {contextType.Name}. " +
                    $"Created instance type: {contextInstance?.GetType().Name ?? "null"}");
            }

            using (context)
            {
                // Aplicar migrations
                var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();

                if (pendingMigrations.Any())
                {
                    _logger.LogInformation("📦 {Module}: {Count} migrations pendentes", moduleName, pendingMigrations.Count);
                    foreach (var migration in pendingMigrations)
                    {
                        _logger.LogDebug("   - {Migration}", migration);
                    }

                    await context.Database.MigrateAsync(cancellationToken);
                    _logger.LogInformation("✅ {Module}: Migrations aplicadas com sucesso", moduleName);
                }
                else
                {
                    _logger.LogInformation("✓ {Module}: Nenhuma migration pendente", moduleName);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao aplicar migrations para {Module}", moduleName);
            throw new InvalidOperationException(
                $"Failed to apply database migrations for module '{moduleName}' (DbContext: {contextType.Name})",
                ex);
        }
    }

    private string ExtractModuleName(Type contextType)
    {
        // Extrai nome do módulo do namespace (ex: MeAjudaAi.Modules.Users.Infrastructure.Persistence.UsersDbContext -> Users)
        var namespaceParts = contextType.Namespace?.Split('.') ?? Array.Empty<string>();
        var moduleIndex = Array.IndexOf(namespaceParts, "Modules");

        if (moduleIndex >= 0 && moduleIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[moduleIndex + 1];
        }

        return contextType.Name.Replace("DbContext", "");
    }
}
