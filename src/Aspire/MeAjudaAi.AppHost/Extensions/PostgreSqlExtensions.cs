using MeAjudaAi.AppHost.Helpers;
using MeAjudaAi.AppHost.Options;
using MeAjudaAi.AppHost.Results;

namespace MeAjudaAi.AppHost.Extensions;

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
        var options = new MeAjudaAiPostgreSqlOptions
        {
            Username = string.Empty,
            Password = string.Empty
        };

        // Aplica sobrescritas de variáveis de ambiente primeiro (consistente com o caminho local/teste)
        ApplyEnvironmentVariables(options);

        // Depois aplica configuração do usuário (pode sobrescrever variáveis de ambiente)
        configure?.Invoke(options);

        // Validação de credenciais para Azure PostgreSQL
        if (string.IsNullOrWhiteSpace(options.Username))
        {
            throw new InvalidOperationException(
                "Azure PostgreSQL username is required. Configure via options.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Azure PostgreSQL password is required. Configure via options or use managed identity.");
        }

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
        if (string.IsNullOrWhiteSpace(options.Username))
            throw new InvalidOperationException("PostgreSQL username cannot be empty. Configure via options or environment variables.");

        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException("POSTGRES_PASSWORD must be provided via env var or options for testing.");

        // Verificar se está rodando em ambiente CI com serviço PostgreSQL externo
        var isCI = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

        if (isCI)
        {
            // Em CI, usar serviço PostgreSQL externo (ex: serviço no GitHub Actions)
            var externalDbHost = Environment.GetEnvironmentVariable("CI_POSTGRES_HOST") ?? "localhost";
            var externalDbPort = Environment.GetEnvironmentVariable("CI_POSTGRES_PORT") ?? "5432";
            var connectionString = $"Host={externalDbHost};Port={externalDbPort};Database={options.MainDatabase};Username={options.Username};Password={options.Password}";

            // Definir a string de conexão como variável de ambiente para que AddConnectionString possa encontrá-la
            Environment.SetEnvironmentVariable("ConnectionStrings__meajudaai-db-local", connectionString);

            // Criar um recurso de string de conexão que lerá da variável de ambiente
            var externalDb = builder.AddConnectionString("meajudaai-db-local");

            return new MeAjudaAiPostgreSqlResult
            {
                MainDatabase = externalDb
            };
        }
        else
        {
            // Testes locais - criar container PostgreSQL com extensão PostGIS
            var postgres = builder.AddPostgres("postgres-local")
                .WithImage("postgis/postgis")
                .WithImageTag("16-3.4") // PostgreSQL 16 com PostGIS 3.4
                .WithEnvironment("POSTGRES_DB", options.MainDatabase)
                .WithEnvironment("POSTGRES_USER", options.Username)
                .WithEnvironment("POSTGRES_PASSWORD", options.Password);

            // Montar scripts de inicialização do banco de dados
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
        if (string.IsNullOrWhiteSpace(options.Username))
            throw new InvalidOperationException("PostgreSQL username is required. Configure via options or environment variables.");

        if (string.IsNullOrWhiteSpace(options.Password))
            throw new InvalidOperationException("POSTGRES_PASSWORD must be provided via env var or options for development.");

        // Criar parameters explícitos para garantir que Aspire use as credenciais corretas nos health checks
        var postgresUserParam = builder.AddParameter("PostgresUser-Dev", options.Username);
        var postgresPasswordParam = builder.AddParameter("PostgresPassword-Dev", options.Password, secret: true);

        // Setup completo de desenvolvimento com PostGIS para geospatial queries
        // NOTA: Sem .WithDataVolume() em desenvolvimento - Docker criará volumes anônimos que serão removidos com containers
        var postgresBuilder = builder.AddPostgres("postgres-local", 
                userName: postgresUserParam, 
                password: postgresPasswordParam, 
                port: 5432) // Porta fixa para Keycloak poder conectar
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
        // - schema users (módulo Usuários - autenticação e perfis)
        // - schema providers (módulo Provedores - prestadores de serviço)
        // - schema documents (módulo Documentos - upload e verificação)
        // - schema search (módulo SearchProviders - busca geoespacial com PostGIS)
        // - schema locations (módulo Localizações - CEP lookup e geocoding)
        // - schema catalogs (módulo Catálogos - catálogo de serviços)
        // - schema bookings (módulo Agendamentos - agendamento de serviços)
        // - schema payments (módulo Pagamentos - gestão financeira)
        // - schema communications (módulo Comunicações - mensagens)
        // - schema hangfire (jobs em segundo plano - Hangfire)
        // - schema identity (Keycloak - autenticação)
        // - schema meajudaai_app (objetos transversais)
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
