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
    public string Password { get; set; } = "dev123";
    
    /// <summary>
    /// Indica se deve habilitar configuração otimizada para testes
    /// </summary>
    public bool IsTestEnvironment { get; set; }
    
    /// <summary>
    /// Indica se deve incluir PgAdmin para desenvolvimento
    /// </summary>
    public bool IncludePgAdmin { get; set; } = true;
    
    /// <summary>
    /// Indica se deve usar isolamento por schemas (padrão: true)
    /// </summary>
    public bool UseSchemaIsolation { get; set; } = true;
}

/// <summary>
/// Resultado da configuração do PostgreSQL contendo referências ao banco de dados
/// </summary>
public sealed class MeAjudaAiPostgreSqlResult
{
    /// <summary>
    /// Referência ao banco de dados principal da aplicação (único para todos os módulos)
    /// </summary>
    public required object MainDatabase { get; init; }
    
    /// <summary>
    /// String de conexão direta (cenários de teste)
    /// </summary>
    public string? DirectConnectionString { get; init; }
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
        configure?.Invoke(options);

        var postgresUserParam = builder.AddParameter("PostgresUser", options.Username);
        var postgresPasswordParam = builder.AddParameter("PostgresPassword", secret: true);

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
        // Use consistent naming with integration tests - they expect "postgres-local"
        var postgres = builder.AddPostgres("postgres-local")
            .WithImageTag("13-alpine") // Use PostgreSQL 13 for better compatibility
            .WithEnvironment("POSTGRES_DB", options.MainDatabase)
            .WithEnvironment("POSTGRES_USER", options.Username)
            .WithEnvironment("POSTGRES_PASSWORD", options.Password)
            .WithEnvironment("POSTGRES_HOST_AUTH_METHOD", "trust"); // Trust authentication for tests

        var mainDb = postgres.AddDatabase("meajudaai-db-local", options.MainDatabase);

        return new MeAjudaAiPostgreSqlResult
        {
            MainDatabase = mainDb,
            DirectConnectionString = null
        };
    }

    private static MeAjudaAiPostgreSqlResult AddDevelopmentPostgreSQL(
        IDistributedApplicationBuilder builder, 
        MeAjudaAiPostgreSqlOptions options)
    {
        // Full-featured development setup
        var postgresBuilder = builder.AddPostgres("postgres-local")
            .WithDataVolume()
            .WithEnvironment("POSTGRES_DB", options.MainDatabase)
            .WithEnvironment("POSTGRES_USER", options.Username)
            .WithEnvironment("POSTGRES_PASSWORD", options.Password)
            .WithEnvironment("PGPASSWORD", options.Password);

        if (options.IncludePgAdmin)
        {
            postgresBuilder.WithPgAdmin();
        }

        var mainDb = postgresBuilder.AddDatabase("meajudaai-db-local", options.MainDatabase);
        
        // Single database approach - all modules use the same database with different schemas
        // - users schema (Users module)
        // - identity schema (Keycloak) 
        // - public schema (shared/common tables)
        // - future modules will get their own schemas

        return new MeAjudaAiPostgreSqlResult
        {
            MainDatabase = mainDb
        };
    }

    private static void ApplyEnvironmentVariables(MeAjudaAiPostgreSqlOptions options)
    {
        // Apply environment variable overrides
        if (Environment.GetEnvironmentVariable("POSTGRES_USER") is string user && !string.IsNullOrEmpty(user))
            options.Username = user;
            
        if (Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") is string password && !string.IsNullOrEmpty(password))
            options.Password = password;
            
        if (Environment.GetEnvironmentVariable("POSTGRES_DB") is string database && !string.IsNullOrEmpty(database))
            options.MainDatabase = database;
    }

    private static bool IsTestEnvironment(IDistributedApplicationBuilder builder)
    {
        return builder.Environment.EnvironmentName == "Testing" ||
               Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";
    }
}