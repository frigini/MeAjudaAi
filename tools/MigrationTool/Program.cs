using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace MeAjudaAi.Tools.MigrationTool;

/// <summary>
/// Ferramenta CLI para aplicar todas as migrações de todos os módulos automaticamente.
/// Uso: dotnet run --project tools/MigrationTool -- [comando]
/// 
/// Comandos disponíveis:
/// - migrate: Aplica todas as migrações pendentes
/// - create: Cria os bancos de dados se não existirem
/// - reset: Remove e recria todos os bancos
/// - status: Mostra o status das migrações
/// </summary>
class Program
{
    private static readonly Dictionary<string, string> _connectionStrings = new()
    {
        ["Users"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123",
        ["Providers"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123",
        ["Documents"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123",
        ["Services"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123",
        ["Orders"] = "Host=localhost;Port=5432;Database=meajudaai;Username=postgres;Password=test123"
    };

    static async Task Main(string[] args)
    {
        var command = args.Length > 0 ? args[0].ToLower() : "migrate";
        
        Console.WriteLine("🔧 MeAjudaAi Migration Tool");
        Console.WriteLine($"📋 Comando: {command}");
        Console.WriteLine();

        var host = CreateHostBuilder(args).Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();
        
        try
        {
            switch (command)
            {
                case "migrate":
                    await ApplyAllMigrationsAsync(host.Services, logger);
                    break;
                case "create":
                    await CreateAllDatabasesAsync(host.Services, logger);
                    break;
                case "reset":
                    await ResetAllDatabasesAsync(host.Services, logger);
                    break;
                case "status":
                    await ShowMigrationStatusAsync(host.Services, logger);
                    break;
                default:
                    ShowUsage();
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Erro durante execução do comando {Command}", command);
            Environment.ExitCode = 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Register all discovered DbContexts
                RegisterAllDbContexts(services);
                
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            });

    private static void RegisterAllDbContexts(IServiceCollection services)
    {
        var dbContextTypes = DiscoverAllDbContextTypes();
        
        foreach (var contextInfo in dbContextTypes)
        {
            var connectionString = GetConnectionStringForModule(contextInfo.ModuleName);
            
            // Use reflection to call AddDbContext<TContext> with the discovered type
            var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions)
                .GetMethod(nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext), 
                    new[] { typeof(IServiceCollection), typeof(Action<DbContextOptionsBuilder>), typeof(ServiceLifetime), typeof(ServiceLifetime) })
                ?.MakeGenericMethod(contextInfo.Type);
            
            addDbContextMethod?.Invoke(null, new object[] 
            { 
                services, 
                new Action<DbContextOptionsBuilder>(options =>
                {
                    options.UseNpgsql(connectionString, npgsqlOptions =>
                    {
                        npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", contextInfo.SchemaName);
                    });
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                }),
                ServiceLifetime.Scoped,
                ServiceLifetime.Scoped
            });
        }
    }

    private static async Task ApplyAllMigrationsAsync(IServiceProvider services, ILogger logger)
    {
        logger.LogInformation("🚀 Aplicando todas as migrações...");
        
        var contexts = GetAllDbContexts(services);
        var totalSuccess = 0;
        var totalFailed = 0;

        foreach (var (contextName, context) in contexts)
        {
            try
            {
                logger.LogInformation("📦 Processando {Context}...", contextName);
                
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                
                logger.LogInformation("  📊 Migrações aplicadas: {Applied}", appliedMigrations.Count());
                logger.LogInformation("  ⏳ Migrações pendentes: {Pending}", pendingMigrations.Count());
                
                if (pendingMigrations.Any())
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("  ✅ Migrações aplicadas com sucesso!");
                }
                else
                {
                    logger.LogInformation("  ℹ️  Nenhuma migração pendente");
                }
                
                totalSuccess++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "  ❌ Erro ao aplicar migrações para {Context}", contextName);
                totalFailed++;
            }
        }
        
        logger.LogInformation("");
        logger.LogInformation("📈 Resumo: {Success} sucessos, {Failed} falhas", totalSuccess, totalFailed);
    }

    private static async Task CreateAllDatabasesAsync(IServiceProvider services, ILogger logger)
    {
        logger.LogInformation("🏗️  Criando todos os bancos de dados...");
        
        var contexts = GetAllDbContexts(services);
        
        foreach (var (contextName, context) in contexts)
        {
            try
            {
                logger.LogInformation("📦 Criando banco para {Context}...", contextName);
                
                var created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("  ✅ Banco criado com sucesso!");
                }
                else
                {
                    logger.LogInformation("  ℹ️  Banco já existe");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "  ❌ Erro ao criar banco para {Context}", contextName);
            }
        }
    }

    private static async Task ResetAllDatabasesAsync(IServiceProvider services, ILogger logger)
    {
        logger.LogWarning("⚠️  ATENÇÃO: Esta operação irá REMOVER todos os dados!");
        logger.LogInformation("Pressione 'Y' para confirmar ou qualquer outra tecla para cancelar...");
        
        var key = Console.ReadKey();
        Console.WriteLine();
        
        if (key.Key != ConsoleKey.Y)
        {
            logger.LogInformation("❌ Operação cancelada pelo usuário");
            return;
        }
        
        logger.LogInformation("🗑️  Removendo e recriando todos os bancos...");
        
        var contexts = GetAllDbContexts(services);
        
        foreach (var (contextName, context) in contexts)
        {
            try
            {
                logger.LogInformation("📦 Resetando {Context}...", contextName);
                
                await context.Database.EnsureDeletedAsync();
                logger.LogInformation("  🗑️  Banco removido");
                
                await context.Database.MigrateAsync();
                logger.LogInformation("  ✅ Banco recriado com migrações");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "  ❌ Erro ao resetar {Context}", contextName);
            }
        }
    }

    private static async Task ShowMigrationStatusAsync(IServiceProvider services, ILogger logger)
    {
        logger.LogInformation("📊 Status das migrações por módulo:");
        logger.LogInformation("");
        
        var contexts = GetAllDbContexts(services);
        
        foreach (var (contextName, context) in contexts)
        {
            try
            {
                logger.LogInformation("📦 {Context}:", contextName);
                
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogWarning("  ❌ Não é possível conectar ao banco");
                    continue;
                }
                
                var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                
                logger.LogInformation("  ✅ Migrações aplicadas: {Count}", appliedMigrations.Count());
                foreach (var migration in appliedMigrations.TakeLast(3))
                {
                    logger.LogInformation("    - {Migration}", migration);
                }
                
                if (pendingMigrations.Any())
                {
                    logger.LogWarning("  ⏳ Migrações pendentes: {Count}", pendingMigrations.Count());
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogWarning("    - {Migration}", migration);
                    }
                }
                else
                {
                    logger.LogInformation("  ✅ Todas as migrações estão aplicadas");
                }
                
                logger.LogInformation("");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "  ❌ Erro ao verificar status de {Context}", contextName);
            }
        }
    }

    private static Dictionary<string, DbContext> GetAllDbContexts(IServiceProvider services)
    {
        var contexts = new Dictionary<string, DbContext>();
        var contextTypes = DiscoverAllDbContextTypes();
        
        foreach (var contextInfo in contextTypes)
        {
            try
            {
                var context = services.GetService(contextInfo.Type) as DbContext;
                if (context != null)
                {
                    contexts[contextInfo.Type.Name] = context;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Não foi possível obter contexto {contextInfo.Type.Name}: {ex.Message}");
            }
        }
        
        return contexts;
    }

    private static List<(Type Type, string ModuleName, string SchemaName)> DiscoverAllDbContextTypes()
    {
        var contextTypes = new List<(Type, string, string)>();
        
        // Load assemblies from the solution
        var solutionRoot = FindSolutionRoot();
        if (solutionRoot != null)
        {
            LoadAssembliesFromSolution(solutionRoot);
        }
        
        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && 
                       a.FullName?.Contains("MeAjudaAi") == true &&
                       a.FullName?.Contains("Infrastructure") == true);

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && 
                               !t.IsAbstract && 
                               typeof(DbContext).IsAssignableFrom(t) &&
                               t.Name.EndsWith("DbContext"))
                    .ToList();

                foreach (var type in types)
                {
                    var moduleName = ExtractModuleName(type);
                    var schemaName = moduleName.ToLowerInvariant();
                    contextTypes.Add((type, moduleName, schemaName));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Erro ao escanear assembly {assembly.FullName}: {ex.Message}");
            }
        }
        
        return contextTypes;
    }

    private static string ExtractModuleName(Type contextType)
    {
        // Extract module name from namespace or type name
        // e.g., MeAjudaAi.Modules.Users.Infrastructure.UsersDbContext -> Users
        var namespaceParts = contextType.Namespace?.Split('.') ?? Array.Empty<string>();
        var moduleIndex = Array.IndexOf(namespaceParts, "Modules");
        
        if (moduleIndex >= 0 && moduleIndex + 1 < namespaceParts.Length)
        {
            return namespaceParts[moduleIndex + 1];
        }
        
        // Fallback: extract from type name
        var typeName = contextType.Name;
        if (typeName.EndsWith("DbContext"))
        {
            return typeName.Substring(0, typeName.Length - "DbContext".Length);
        }
        
        return "Unknown";
    }

    private static string GetConnectionStringForModule(string moduleName)
    {
        if (_connectionStrings.TryGetValue(moduleName, out var connectionString))
        {
            return connectionString;
        }
        
        // Fallback: generate connection string
        var dbName = $"meajudaai_{moduleName.ToLowerInvariant()}";
        return $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
    }

    private static string? FindSolutionRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.GetFiles(currentDir, "*.sln").Any())
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        return null;
    }

    private static void LoadAssembliesFromSolution(string solutionRoot)
    {
        try
        {
            var infrastructureAssemblies = Directory.GetFiles(
                Path.Combine(solutionRoot, "src"), 
                "*Infrastructure*.dll", 
                SearchOption.AllDirectories);
            
            foreach (var assemblyPath in infrastructureAssemblies)
            {
                try
                {
                    Assembly.LoadFrom(assemblyPath);
                }
                catch
                {
                    // Ignore assembly load errors
                }
            }
        }
        catch
        {
            // Ignore directory errors
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Uso: dotnet run --project tools/MigrationTool -- [comando]");
        Console.WriteLine();
        Console.WriteLine("Comandos disponíveis:");
        Console.WriteLine("  migrate  - Aplica todas as migrações pendentes (padrão)");
        Console.WriteLine("  create   - Cria os bancos de dados se não existirem");
        Console.WriteLine("  reset    - Remove e recria todos os bancos");
        Console.WriteLine("  status   - Mostra o status das migrações");
        Console.WriteLine();
        Console.WriteLine("Exemplos:");
        Console.WriteLine("  dotnet run --project tools/MigrationTool");
        Console.WriteLine("  dotnet run --project tools/MigrationTool -- status");
        Console.WriteLine("  dotnet run --project tools/MigrationTool -- reset");
    }
}
