using Aspire.Hosting;
using MeAjudaAi.AppHost.Extensions;
using MeAjudaAi.AppHost.Helpers;

namespace MeAjudaAi.AppHost;

internal static class Program
{
    public static void Main(string[] args)
    {
        // Configurar caminhos do Aspire para pacotes NuGet locais
        // Workaround para: https://github.com/dotnet/aspire/issues/6789
        ConfigureAspireLocalPackages();
        
        var builder = DistributedApplication.CreateBuilder(args);

        var isTestingEnv = EnvironmentHelpers.IsTesting(builder);

        // Log ambiente detectado para debug
        var detectedEnv = EnvironmentHelpers.GetEnvironmentName(builder);
        Console.WriteLine($"🔍 Detected environment: '{detectedEnv}' (IsTesting: {isTestingEnv}, IsDevelopment: {EnvironmentHelpers.IsDevelopment(builder)}, IsProduction: {EnvironmentHelpers.IsProduction(builder)})");

        if (isTestingEnv)
        {
            Console.WriteLine("⚙️  Configuring TEST environment");
            ConfigureTestingEnvironment(builder);
        }
        else if (EnvironmentHelpers.IsDevelopment(builder))
        {
            Console.WriteLine("⚙️  Configuring DEVELOPMENT environment");
            ConfigureDevelopmentEnvironment(builder);
        }
        else if (EnvironmentHelpers.IsProduction(builder))
        {
            Console.WriteLine("⚙️  Configuring PRODUCTION environment");
            ConfigureProductionEnvironment(builder);
        }
        else
        {
            var currentEnv = EnvironmentHelpers.GetEnvironmentName(builder);
            var errorMessage = $"Unsupported environment: '{currentEnv}'. Only Testing, Development, and Production environments are supported.";

            Console.Error.WriteLine($"ERROR: {errorMessage}");
            Environment.Exit(1);
        }

        builder.Build().Run();
    }

    private static void ConfigureTestingEnvironment(IDistributedApplicationBuilder builder)
    {
        var testDbName = Environment.GetEnvironmentVariable("MEAJUDAAI_DB") ?? "meajudaai";
        var testDbUser = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_USER") ?? "postgres";
        var testDbPassword = Environment.GetEnvironmentVariable("MEAJUDAAI_DB_PASS") ?? string.Empty;

        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        var isDryRun = Environment.GetCommandLineArgs().Contains("--dry-run") || Environment.GetCommandLineArgs().Contains("--publisher");

        if (string.IsNullOrEmpty(testDbPassword))
        {
            if (isCI && !isDryRun)
            {
                Console.Error.WriteLine("ERROR: MEAJUDAAI_DB_PASS environment variable is required in CI but not set.");
                Console.Error.WriteLine("Please set MEAJUDAAI_DB_PASS to the database password in your CI environment.");
                Environment.Exit(1);
            }
            testDbPassword = "test123";
        }

        var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
        {
            options.IsTestEnvironment = true;
            options.MainDatabase = testDbName;
            options.Username = testDbUser;
            options.Password = testDbPassword;
        });

        // Aplicar migrations automaticamente (Testing também)
        postgresql.MainDatabase.WithMigrations();

        var redis = builder.AddRedis("redis");

        _ = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
            .WithReference(postgresql.MainDatabase, "DefaultConnection")
            .WithReference(redis)
            .WaitFor(postgresql.MainDatabase)
            .WaitFor(redis)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Testing")
            .WithEnvironment("Logging__LogLevel__Default", "Information")
            .WithEnvironment("Logging__LogLevel__Microsoft.EntityFrameworkCore", "Warning")
            .WithEnvironment("Logging__LogLevel__Microsoft.Hosting.Lifetime", "Information")
            .WithEnvironment("Keycloak__Enabled", "false")
            .WithEnvironment("RabbitMQ__Enabled", "false")
            .WithEnvironment("HealthChecks__Timeout", "30");
    }

    private static void ConfigureDevelopmentEnvironment(IDistributedApplicationBuilder builder)
    {
        var mainDatabase = Environment.GetEnvironmentVariable("MAIN_DATABASE") ?? "meajudaai";
        var dbUsername = Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres";
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? string.Empty;
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
        if (string.IsNullOrEmpty(dbPassword))
        {
            if (isCI)
            {
                Console.Error.WriteLine("ERROR: DB_PASSWORD environment variable is required in CI but not set.");
                Console.Error.WriteLine("Please set DB_PASSWORD to the database password in your CI environment.");
                Environment.Exit(1);
            }
            dbPassword = "test123";
        }
        var includePgAdminStr = Environment.GetEnvironmentVariable("INCLUDE_PGADMIN") ?? "true";
        var includePgAdmin = !bool.TryParse(includePgAdminStr, out var pgAdminResult) || pgAdminResult;

        var postgresql = builder.AddMeAjudaAiPostgreSQL(options =>
        {
            options.MainDatabase = mainDatabase;
            options.Username = dbUsername;
            options.Password = dbPassword;
            options.IncludePgAdmin = includePgAdmin;
        });

        // Aplicar migrations automaticamente
        postgresql.MainDatabase.WithMigrations();

        var redis = builder.AddRedis("redis");

        var rabbitMq = builder.AddRabbitMQ("rabbitmq");

        var keycloak = builder.AddMeAjudaAiKeycloak(options =>
        {
            // OBRIGATÓRIO: AdminUsername e AdminPassword
            options.AdminUsername = builder.Configuration["Keycloak:AdminUsername"]
                                   ?? Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_USERNAME")
                                   ?? "admin"; // Fallback apenas para desenvolvimento local
            
            var adminPassword = builder.Configuration["Keycloak:AdminPassword"]
                                ?? Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
            var isKeycloakCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));
            if (string.IsNullOrEmpty(adminPassword))
            {
                if (isKeycloakCI)
                {
                    Console.Error.WriteLine("ERROR: KEYCLOAK_ADMIN_PASSWORD environment variable is required in CI but not set.");
                    Console.Error.WriteLine("Please set KEYCLOAK_ADMIN_PASSWORD to the Keycloak admin password in your CI environment.");
                    Environment.Exit(1);
                }
                adminPassword = "admin123"; // Fallback apenas para desenvolvimento local
            }
            options.AdminPassword = adminPassword;
            options.DatabaseHost = builder.Configuration["Keycloak:DatabaseHost"]
                                  ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_HOST")
                                  ?? "postgres-local";
            options.DatabasePort = builder.Configuration["Keycloak:DatabasePort"]
                                  ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_PORT")
                                  ?? "5432";
            options.DatabaseName = builder.Configuration["Keycloak:DatabaseName"]
                                  ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_NAME")
                                  ?? mainDatabase;
            options.DatabaseSchema = builder.Configuration["Keycloak:DatabaseSchema"]
                                     ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_SCHEMA")
                                     ?? "identity";
            options.DatabaseUsername = builder.Configuration["Keycloak:DatabaseUsername"]
                                       ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_USER")
                                       ?? dbUsername;
            options.DatabasePassword = builder.Configuration["Keycloak:DatabasePassword"]
                                       ?? Environment.GetEnvironmentVariable("KEYCLOAK_DB_PASSWORD")
                                       ?? dbPassword;

            var exposeHttpStr = builder.Configuration["Keycloak:ExposeHttpEndpoint"]
                               ?? Environment.GetEnvironmentVariable("KEYCLOAK_EXPOSE_HTTP");
            options.ExposeHttpEndpoint = !bool.TryParse(exposeHttpStr, out var exposeResult) || exposeResult;
        });

        _ = builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
            .WithReference(postgresql.MainDatabase, "DefaultConnection")
            .WithReference(redis)
            .WaitFor(postgresql.MainDatabase)
            .WaitFor(redis)
            .WithReference(rabbitMq)
            .WaitFor(rabbitMq)
            .WithReference(keycloak.Keycloak)
            .WaitFor(keycloak.Keycloak)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", EnvironmentHelpers.GetEnvironmentName(builder));

        // Admin Portal (Blazor WASM)
        _ = builder.AddProject<Projects.MeAjudaAi_Web_Admin>("admin-portal")
            .WithExternalHttpEndpoints()
            .WithEnvironment("ApiBaseUrl", "https://localhost:7001")
            .WithEnvironment("Keycloak__Authority", "http://localhost:8080/realms/meajudaai")
            .WithEnvironment("Keycloak__ClientId", "admin-portal");
    }

    private static void ConfigureProductionEnvironment(IDistributedApplicationBuilder builder)
    {
        var postgresql = builder.AddMeAjudaAiAzurePostgreSQL(options =>
        {
            options.MainDatabase = "meajudaai";
            options.Username = "postgres";
        });

        var redis = builder.AddRedis("redis");

        var serviceBus = builder.AddAzureServiceBus("servicebus");

        var keycloak = builder.AddMeAjudaAiKeycloakProduction();

        builder.AddProject<Projects.MeAjudaAi_ApiService>("apiservice")
            .WithReference(postgresql.MainDatabase, "DefaultConnection")
            .WithReference(redis)
            .WaitFor(postgresql.MainDatabase)
            .WaitFor(redis)
            .WithReference(serviceBus)
            .WaitFor(serviceBus)
            .WithReference(keycloak.Keycloak)
            .WaitFor(keycloak.Keycloak)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", EnvironmentHelpers.GetEnvironmentName(builder));
    }

    /// <summary>
    /// Configura caminhos do DCP e Dashboard quando usando pacotes NuGet locais.
    /// Workaround para: https://github.com/dotnet/aspire/issues/6789
    /// </summary>
    private static void ConfigureAspireLocalPackages()
    {
        // Detectar diretório da solução procurando por global.json ou arquivo .sln
        var solutionRoot = FindSolutionRoot(AppContext.BaseDirectory);
        if (solutionRoot == null)
        {
            Console.WriteLine("⚠️  Could not locate solution root, skipping local packages configuration");
            return;
        }

        var packagesDir = Path.Combine(solutionRoot, "packages");

        Console.WriteLine($"🔍 Detecting Aspire packages...");
        Console.WriteLine($"   Solution root: {solutionRoot}");
        Console.WriteLine($"   Packages dir: {packagesDir}");
        Console.WriteLine($"   Exists: {Directory.Exists(packagesDir)}");

        if (!Directory.Exists(packagesDir))
        {
            Console.WriteLine("⚠️  Not using local packages, skipping configuration");
            return;
        }

        // Versão sincronizada com Directory.Build.targets
        const string aspireVersion = "13.1.0";

        // Detectar plataforma e arquitetura para RID correto
        var isWindows = OperatingSystem.IsWindows();
        var isLinux = OperatingSystem.IsLinux();
        var isMacOS = OperatingSystem.IsMacOS();
        var isArm64 = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture == System.Runtime.InteropServices.Architecture.Arm64;
        
        string rid;
        string exeExtension;
        
        if (isWindows)
        {
            rid = isArm64 ? "win-arm64" : "win-x64";
            exeExtension = ".exe";
        }
        else if (isLinux)
        {
            rid = isArm64 ? "linux-arm64" : "linux-x64";
            exeExtension = "";
        }
        else if (isMacOS)
        {
            rid = isArm64 ? "osx-arm64" : "osx-x64";
            exeExtension = "";
        }
        else
        {
            Console.WriteLine("⚠️  Unknown platform, skipping local packages configuration");
            return;
        }

        var dcpPath = Path.Combine(packagesDir, $"aspire.hosting.orchestration.{rid}", aspireVersion, "tools", $"dcp{exeExtension}");
        var dashboardDir = Path.Combine(packagesDir, $"aspire.dashboard.sdk.{rid}", aspireVersion, "tools");
        var dashboardPath = Path.Combine(dashboardDir, $"Aspire.Dashboard{exeExtension}");

        Console.WriteLine($"   Platform: {rid}");
        Console.WriteLine($"   DCP path: {dcpPath}");
        Console.WriteLine($"   DCP exists: {File.Exists(dcpPath)}");
        Console.WriteLine($"   Dashboard path: {dashboardPath}");
        Console.WriteLine($"   Dashboard exists: {File.Exists(dashboardPath)}");

        if (File.Exists(dcpPath) && File.Exists(dashboardPath))
        {
            // Configurar variáveis de ambiente que o Aspire lê
            Environment.SetEnvironmentVariable("DOTNET_DCP_CLI_PATH", dcpPath);
            Environment.SetEnvironmentVariable("DCP_CLI_PATH", dcpPath);
            Environment.SetEnvironmentVariable("Aspire__CliPath", dcpPath);
            
            Environment.SetEnvironmentVariable("DOTNET_ASPIRE_DASHBOARD_PATH", dashboardDir);
            Environment.SetEnvironmentVariable("ASPIRE_DASHBOARD_PATH", dashboardDir);
            Environment.SetEnvironmentVariable("Aspire__DashboardPath", dashboardDir);
            
            Console.WriteLine("✅ Aspire local packages configured:");
            Console.WriteLine($"   DCP: {dcpPath}");
            Console.WriteLine($"   Dashboard: {dashboardPath}");
            Console.WriteLine($"   Variables set: DOTNET_DCP_CLI_PATH, DCP_CLI_PATH, Aspire__CliPath");
            Console.WriteLine($"   Variables set: DOTNET_ASPIRE_DASHBOARD_PATH, ASPIRE_DASHBOARD_PATH, Aspire__DashboardPath");
        }
        else
        {
            Console.WriteLine("⚠️  Warning: Aspire binaries not found in local packages");
            if (!File.Exists(dcpPath))
                Console.WriteLine($"   Missing: {dcpPath}");
            if (!File.Exists(dashboardPath))
                Console.WriteLine($"   Missing: {dashboardPath}");
        }
    }

    /// <summary>
    /// Procura o diretório raiz da solução procurando por global.json ou arquivos .sln/.slnx
    /// </summary>
    private static string? FindSolutionRoot(string startDir)
    {
        var dir = new DirectoryInfo(startDir);
        while (dir != null)
        {
            // Procurar por global.json (mais confiável para .NET projects)
            if (File.Exists(Path.Combine(dir.FullName, "global.json")))
                return dir.FullName;
            
            // Ou procurar por arquivos de solução (.sln ou .slnx)
            if (dir.GetFiles("*.sln").Length > 0 || dir.GetFiles("*.slnx").Length > 0)
                return dir.FullName;
            
            dir = dir.Parent;
        }
        return null;
    }
}