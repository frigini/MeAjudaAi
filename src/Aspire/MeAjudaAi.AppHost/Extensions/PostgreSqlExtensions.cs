using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using MeAjudaAi.AppHost.Helpers;

namespace MeAjudaAi.AppHost.Extensions;

/// <summary>
/// Opções de configuração para o setup do PostgreSQL do MeAjudaAi
/// </summary>
public sealed class MeAjudaAiPostgreSqlOptions
{
    /// <summary>
    /// Nome do banco de dados principal da aplicação (agora único para todos os módulos)
    /// </summary>
    public string MainDatabase { get; set; } = "meajudaai";

    /// <summary>
    /// Usuário do PostgreSQL
    /// </summary>
    public string Username { get; set; } = "postgres";

    /// <summary>
    /// Senha do PostgreSQL
    /// </summary>
    public string Password { get; set; } = "";

    /// <summary>
    /// Indica se deve habilitar configuração otimizada para testes
    /// </summary>
    public bool IsTestEnvironment { get; set; }

    /// <summary>
    /// Indica se deve incluir PgAdmin para desenvolvimento
    /// </summary>
    public bool IncludePgAdmin { get; set; } = true;
}

/// <summary>
/// Resultado da configuração do PostgreSQL contendo referências ao banco de dados
/// </summary>
public sealed class MeAjudaAiPostgreSqlResult
{
    /// <summary>
    /// Referência ao banco de dados principal da aplicação (único para todos os módulos)
    /// </summary>
    public required IResourceBuilder<IResourceWithConnectionString> MainDatabase { get; init; }
}

/// <summary>
/// Métodos de extensão para adicionar configuração do PostgreSQL do MeAjudaAi
/// </summary>
public static class PostgreSqlExtensions
{
    /// <summary>
    /// Adiciona configuração do PostgreSQL otimizada para a aplicação MeAjudaAi.
    /// Detecta automaticamente o ambiente e aplica otimizações apropriadas.
    /// </summary>
    /// <param name="builder">O builder de aplicação distribuída</param>
    /// <param name="configure">Ação de configuração opcional</param>
    /// <returns>Resultado da configuração do PostgreSQL com referências aos bancos</returns>
    public static MeAjudaAiPostgreSqlResult AddMeAjudaAiPostgreSQL(
        this IDistributedApplicationBuilder builder,
        Action<MeAjudaAiPostgreSqlOptions>? configure = null)
    {
        var options = new MeAjudaAiPostgreSqlOptions();

        // Aplica sobrescritas de variáveis de ambiente primeiro
        ApplyEnvironmentVariables(options);

        // Depois aplica configuração do usuário (pode sobrescrever variáveis de ambiente)
        configure?.Invoke(options);

        // Detecta automaticamente ambiente de teste se não estiver explicitamente definido
        if (!options.IsTestEnvironment)
        {
            options.IsTestEnvironment = IsTestEnvironment(builder);
        }

        if (options.IsTestEnvironment)
        {
            return AddTestPostgreSQL(builder, options);
        }
        else
        {
            return AddDevelopmentPostgreSQL(builder, options);
        }
    }

    /// <summary>
    /// Adiciona configuração do Azure PostgreSQL para ambientes de produção
    /// </summary>
    /// <param name="builder">O builder de aplicação distribuída</param>
    /// <param name="configure">Ação de configuração opcional</param>
    /// <returns>Resultado da configuração do PostgreSQL com referências aos bancos</returns>
    public static MeAjudaAiPostgreSqlResult AddMeAjudaAiAzurePostgreSQL(
        this IDistributedApplicationBuilder builder,
        Action<MeAjudaAiPostgreSqlOptions>? configure = null)
    {
        var options = new MeAjudaAiPostgreSqlOptions();

        // Aplica sobrescritas de variáveis de ambiente primeiro (consistente com o caminho local/test)
        ApplyEnvironmentVariables(options);

        // Depois aplica configuração do usuário (pode sobrescrever variáveis de ambiente)
        configure?.Invoke(options);

        var postgresUserParam = builder.AddParameter("PostgresUser", options.Username);
        var postgresPasswordParam = builder.AddParameter("PostgresPassword", options.Password, secret: true);

        var postgresAzure = builder.AddAzurePostgresFlexibleServer("postgres-azure")
            .WithPasswordAuthentication(
                userName: postgresUserParam,
                password: postgresPasswordParam);

        var mainDb = postgresAzure.AddDatabase("meajudaai-db-azure", options.MainDatabase);

        return new MeAjudaAiPostgreSqlResult
        {
            MainDatabase = mainDb
        };
    }

    private static MeAjudaAiPostgreSqlResult AddTestPostgreSQL(
        IDistributedApplicationBuilder builder,
        MeAjudaAiPostgreSqlOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException("POSTGRES_PASSWORD must be provided via env var or options for testing.");

        // Check if running in CI environment with external PostgreSQL service
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        if (isCI)
        {
            // In CI, use external PostgreSQL service (e.g., GitHub Actions service)
            var externalDbHost = Environment.GetEnvironmentVariable("CI_POSTGRES_HOST") ?? "localhost";
            var externalDbPort = Environment.GetEnvironmentVariable("CI_POSTGRES_PORT") ?? "5432";
            var connectionString = $"Host={externalDbHost};Port={externalDbPort};Database={options.MainDatabase};Username={options.Username};Password={options.Password}";

            // Set the connection string as an environment variable so AddConnectionString can find it
            Environment.SetEnvironmentVariable("ConnectionStrings__meajudaai-db-local", connectionString);

            // Create a connection string resource that will read from the environment variable
            var externalDb = builder.AddConnectionString("meajudaai-db-local");

            return new MeAjudaAiPostgreSqlResult
            {
                MainDatabase = externalDb
            };
        }
        else
        {
            // Local testing - create PostgreSQL container with PostGIS extension
            var postgres = builder.AddPostgres("postgres-local")
                .WithImage("postgis/postgis")
                .WithImageTag("16-3.4") // PostgreSQL 16 with PostGIS 3.4
                .WithEnvironment("POSTGRES_DB", options.MainDatabase)
                .WithEnvironment("POSTGRES_USER", options.Username)
                .WithEnvironment("POSTGRES_PASSWORD", options.Password);

            // Mount database initialization scripts
            MountInitializationScripts(postgres, builder);

            var mainDb = postgres.AddDatabase("meajudaai-db-local", options.MainDatabase);

            return new MeAjudaAiPostgreSqlResult
            {
                MainDatabase = mainDb
            };
        }
    }

    private static MeAjudaAiPostgreSqlResult AddDevelopmentPostgreSQL(
        IDistributedApplicationBuilder builder,
        MeAjudaAiPostgreSqlOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException("POSTGRES_PASSWORD must be provided via env var or options for development.");

        // Setup completo de desenvolvimento com PostGIS para geospatial queries
        var postgresBuilder = builder.AddPostgres("postgres-local")
            .WithDataVolume()
            .WithImage("postgis/postgis")
            .WithImageTag("16-3.4") // PostgreSQL 16 with PostGIS 3.4
            .WithEnvironment("POSTGRES_DB", options.MainDatabase)
            .WithEnvironment("POSTGRES_USER", options.Username)
            .WithEnvironment("POSTGRES_PASSWORD", options.Password);

        // Mount database initialization scripts
        MountInitializationScripts(postgresBuilder, builder);

        if (options.IncludePgAdmin)
        {
            postgresBuilder.WithPgAdmin();
        }

        var mainDb = postgresBuilder.AddDatabase("meajudaai-db-local", options.MainDatabase);

        // Abordagem de banco único - todos os módulos usam o mesmo banco com schemas diferentes
        // - schema users (módulo Users - autenticação e perfis)
        // - schema providers (módulo Providers - prestadores de serviço)
        // - schema documents (módulo Documents - upload e verificação)
        // - schema search (módulo Search - busca geoespacial com PostGIS)
        // - schema location (módulo Location - CEP lookup e geocoding)
        // - schema catalogs (módulo Catalogs - catálogo de serviços)
        // - schema hangfire (background jobs - Hangfire)
        // - schema identity (Keycloak - autenticação)
        // - schema meajudaai_app (cross-cutting objects)
        // - schema public (tabelas compartilhadas/comuns)

        return new MeAjudaAiPostgreSqlResult
        {
            MainDatabase = mainDb
        };
    }

    private static void ApplyEnvironmentVariables(MeAjudaAiPostgreSqlOptions options)
    {
        // Aplica sobrescritas de variáveis de ambiente
        if (Environment.GetEnvironmentVariable("POSTGRES_USER") is string user && !string.IsNullOrEmpty(user))
            options.Username = user;

        if (Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") is string password && !string.IsNullOrEmpty(password))
            options.Password = password;

        if (Environment.GetEnvironmentVariable("POSTGRES_DB") is string database && !string.IsNullOrEmpty(database))
            options.MainDatabase = database;
    }

    private static bool IsTestEnvironment(IDistributedApplicationBuilder builder)
    {
        return EnvironmentHelpers.IsTesting(builder);
    }

    /// <summary>
    /// Monta os scripts de inicialização do banco de dados no container PostgreSQL
    /// </summary>
    private static void MountInitializationScripts(
        IResourceBuilder<PostgresServerResource> postgresBuilder,
        IDistributedApplicationBuilder builder)
    {
        // Caminho relativo dos scripts de inicialização
        // De: src/Aspire/MeAjudaAi.AppHost
        // Para: infrastructure/database
        var appDirectory = builder.AppHostDirectory;
        var infrastructurePath = Path.Combine(appDirectory, "..", "..", "..", "infrastructure", "database");
        var absolutePath = Path.GetFullPath(infrastructurePath);

        if (!Directory.Exists(absolutePath))
        {
            Console.WriteLine($"WARNING: Database initialization scripts not found at: {absolutePath}");
            Console.WriteLine("Database will be created without initial roles and permissions.");
            return;
        }

        // Monta a pasta database como /docker-entrypoint-initdb.d
        // PostgreSQL executa automaticamente scripts .sql e .sh nessa pasta na inicialização
        postgresBuilder.WithBindMount(absolutePath, "/docker-entrypoint-initdb.d", isReadOnly: true);

        Console.WriteLine($"INFO: Mounted database initialization scripts from: {absolutePath}");
    }
}
