using System.Reflection;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        builder.ApplicationBuilder.Services.AddHostedService<MigrationHostedService>();
        return builder;
    }
}

/// <summary>
/// Hosted service que roda migrations na inicialização do AppHost
/// </summary>
internal class MigrationHostedService : IHostedService
{
    private readonly ILogger<MigrationHostedService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public MigrationHostedService(
        ILogger<MigrationHostedService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔄 Iniciando migrations de todos os módulos...");

        try
        {
            var connectionString = GetConnectionString();
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("⚠️ Connection string não encontrada, pulando migrations");
                return;
            }

            var dbContextTypes = DiscoverDbContextTypes();
            _logger.LogInformation("📋 Encontrados {Count} DbContexts para migração", dbContextTypes.Count);

            foreach (var contextType in dbContextTypes)
            {
                await MigrateDbContextAsync(contextType, connectionString, cancellationToken);
            }

            _logger.LogInformation("✅ Todas as migrations foram aplicadas com sucesso!");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erro ao aplicar migrations");
            throw;
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

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }

    private List<Type> DiscoverDbContextTypes()
    {
        var dbContextTypes = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName?.Contains("MeAjudaAi.Modules") == true)
            .ToList();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(DbContext).IsAssignableFrom(t))
                    .Where(t => t.Name.EndsWith("DbContext"))
                    .ToList();

                dbContextTypes.AddRange(types);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Erro ao descobrir tipos no assembly {AssemblyName}", assembly.FullName);
            }
        }

        return dbContextTypes;
    }

    private async Task MigrateDbContextAsync(Type contextType, string connectionString, CancellationToken cancellationToken)
    {
        var moduleName = ExtractModuleName(contextType);
        _logger.LogInformation("🔧 Aplicando migrations para {Module}...", moduleName);

        try
        {
            // Criar DbContextOptionsBuilder dinamicamente
            var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(contextType);
            var optionsBuilder = Activator.CreateInstance(optionsBuilderType) as DbContextOptionsBuilder;

            if (optionsBuilder == null)
            {
                throw new InvalidOperationException($"Não foi possível criar DbContextOptionsBuilder para {contextType.Name}");
            }

            // Configurar PostgreSQL
            optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(contextType.Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
            });

            // Criar instância do DbContext
            var context = Activator.CreateInstance(contextType, optionsBuilder.Options) as DbContext;

            if (context == null)
            {
                throw new InvalidOperationException($"Não foi possível criar instância de {contextType.Name}");
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
            throw;
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
