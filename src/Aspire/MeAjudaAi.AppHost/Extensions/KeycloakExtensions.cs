namespace MeAjudaAi.AppHost.Extensions;

/// <summary>
/// Opções de configuração para o setup do Keycloak do MeAjudaAi
/// </summary>
public sealed class MeAjudaAiKeycloakOptions
{
    /// <summary>
    /// Nome de usuário do administrador do Keycloak
    /// </summary>
    public string AdminUsername { get; set; } = "admin";
    
    /// <summary>
    /// Senha do administrador do Keycloak
    /// </summary>
    public string AdminPassword { get; set; } = "admin123";
    
    /// <summary>
    /// Host do banco de dados PostgreSQL
    /// </summary>
    public string DatabaseHost { get; set; } = "postgres-local";
    
    /// <summary>
    /// Porta do banco de dados PostgreSQL
    /// </summary>
    public string DatabasePort { get; set; } = "5432";
    
    /// <summary>
    /// Nome do banco de dados
    /// </summary>
    public string DatabaseName { get; set; } = "meajudaai";
    
    /// <summary>
    /// Schema do banco de dados para o Keycloak (padrão: 'identity')
    /// </summary>
    public string DatabaseSchema { get; set; } = "identity";
    
    /// <summary>
    /// Nome de usuário do banco de dados
    /// </summary>
    public string DatabaseUsername { get; set; } = "postgres";
    
    /// <summary>
    /// Senha do banco de dados
    /// </summary>
    public string DatabasePassword { get; set; } =
        Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "dev123";
    
    /// <summary>
    /// Hostname para URLs de produção (ex: keycloak.mydomain.com)
    /// </summary>
    public string? Hostname { get; set; }
    
    /// <summary>
    /// Indica se deve expor endpoint HTTP (padrão: true para desenvolvimento)
    /// </summary>
    public bool ExposeHttpEndpoint { get; set; } = true;
    
    /// <summary>
    /// Realm a ser importado na inicialização
    /// </summary>
    public string? ImportRealm { get; set; } = "/opt/keycloak/data/import/meajudaai-realm.json";
    
    /// <summary>
    /// Indica se está em ambiente de teste (configurações otimizadas)
    /// </summary>
    public bool IsTestEnvironment { get; set; }
}

/// <summary>
/// Resultado da configuração do Keycloak
/// </summary>
public sealed class MeAjudaAiKeycloakResult
{
    /// <summary>
    /// Referência ao container do Keycloak
    /// </summary>
    public required IResourceBuilder<KeycloakResource> Keycloak { get; init; }
    
    /// <summary>
    /// URL base do Keycloak para autenticação
    /// </summary>
    public required string AuthUrl { get; init; }
    
    /// <summary>
    /// URL de administração do Keycloak
    /// </summary>
    public required string AdminUrl { get; init; }
}

/// <summary>
/// Extensões para configuração do Keycloak no MeAjudaAi
/// </summary>
public static class MeAjudaAiKeycloakExtensions
{
    /// <summary>
    /// Adiciona o Keycloak configurado para desenvolvimento local
    /// </summary>
    public static MeAjudaAiKeycloakResult AddMeAjudaAiKeycloak(
        this IDistributedApplicationBuilder builder,
        Action<MeAjudaAiKeycloakOptions>? configure = null)
    {
        var options = new MeAjudaAiKeycloakOptions();
        configure?.Invoke(options);

        Console.WriteLine($"[Keycloak] Configurando Keycloak para desenvolvimento...");
        Console.WriteLine($"[Keycloak] Database Schema: {options.DatabaseSchema}");
        Console.WriteLine($"[Keycloak] Admin User: {options.AdminUsername}");

        var keycloak = builder.AddKeycloak("keycloak")
            .WithDataVolume()
            // Configurar banco de dados PostgreSQL com schema 'identity'
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL", $"jdbc:postgresql://{options.DatabaseHost}:{options.DatabasePort}/{options.DatabaseName}?currentSchema={options.DatabaseSchema}")
            .WithEnvironment("KC_DB_USERNAME", options.DatabaseUsername)
            .WithEnvironment("KC_DB_PASSWORD", options.DatabasePassword)
            .WithEnvironment("KC_DB_SCHEMA", options.DatabaseSchema)
            // Credenciais do admin
            .WithEnvironment("KEYCLOAK_ADMIN", options.AdminUsername)
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", options.AdminPassword)
            // Configurações de desenvolvimento
            .WithEnvironment("KC_HOSTNAME_STRICT", "false")
            .WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "false")
            .WithEnvironment("KC_HTTP_ENABLED", "true")
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithEnvironment("KC_METRICS_ENABLED", "true");

        // Importar realm na inicialização (apenas se especificado)
        if (!string.IsNullOrEmpty(options.ImportRealm))
        {
            keycloak = keycloak
                .WithEnvironment("KC_IMPORT", options.ImportRealm)
                .WithArgs("start-dev", "--import-realm");
        }
        else
        {
            keycloak = keycloak.WithArgs("start-dev");
        }

        if (options.ExposeHttpEndpoint)
        {
            keycloak = keycloak.WithHttpEndpoint(targetPort: 8080, name: "keycloak-http");
        }

        Console.WriteLine($"[Keycloak] ✅ Keycloak configurado:");
        Console.WriteLine($"[Keycloak]    Porta HTTP: 8080");
        Console.WriteLine($"[Keycloak]    Schema: {options.DatabaseSchema}");

        return new MeAjudaAiKeycloakResult
        {
            Keycloak = keycloak,
            AuthUrl = "http://localhost:8080",
            AdminUrl = "http://localhost:8080/admin"
        };
    }

    /// <summary>
    /// Adiciona o Keycloak configurado para produção (Azure)
    /// </summary>
    public static MeAjudaAiKeycloakResult AddMeAjudaAiKeycloakProduction(
        this IDistributedApplicationBuilder builder,
        Action<MeAjudaAiKeycloakOptions>? configure = null)
    {
        // Registrar parâmetros secretos obrigatórios
        var keycloakAdminPassword = builder.AddParameter("keycloak-admin-password", secret: true);
        var postgresPassword = builder.AddParameter("postgres-password", secret: true);

        // Verificar se as variáveis de ambiente obrigatórias estão definidas
        var adminPasswordFromEnv = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD");
        var dbPasswordFromEnv = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        if (string.IsNullOrEmpty(adminPasswordFromEnv))
        {
            throw new InvalidOperationException(
                "KEYCLOAK_ADMIN_PASSWORD environment variable is required for production deployment. " +
                "Please set this environment variable with a secure password.");
        }

        if (string.IsNullOrEmpty(dbPasswordFromEnv))
        {
            throw new InvalidOperationException(
                "POSTGRES_PASSWORD environment variable is required for production deployment. " +
                "Please set this environment variable with a secure password.");
        }

        var options = new MeAjudaAiKeycloakOptions
        {
            // Configurações seguras para produção - usar valores das variáveis de ambiente validadas
            ExposeHttpEndpoint = false,
            AdminPassword = adminPasswordFromEnv,
            DatabasePassword = dbPasswordFromEnv
        };
        configure?.Invoke(options);

        Console.WriteLine($"[Keycloak] Configurando Keycloak para produção...");
        Console.WriteLine($"[Keycloak] Database Schema: {options.DatabaseSchema}");

        var keycloak = builder.AddKeycloak("keycloak")
            .WithDataVolume()
            // Configurar banco de dados PostgreSQL com schema 'identity'
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL", $"jdbc:postgresql://{options.DatabaseHost}:{options.DatabasePort}/{options.DatabaseName}?currentSchema={options.DatabaseSchema}")
            .WithEnvironment("KC_DB_USERNAME", options.DatabaseUsername)
            .WithEnvironment("KC_DB_PASSWORD", postgresPassword)
            .WithEnvironment("KC_DB_SCHEMA", options.DatabaseSchema)
            // Credenciais do admin usando parâmetros secretos
            .WithEnvironment("KEYCLOAK_ADMIN", options.AdminUsername)
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", keycloakAdminPassword)
            // Configurações de produção
            .WithEnvironment("KC_HOSTNAME_STRICT", "true")
            .WithEnvironment("KC_HOSTNAME_STRICT_HTTPS", "true")
            .WithEnvironment("KC_HTTP_ENABLED", "false")
            .WithEnvironment("KC_HTTPS_PORT", "8443")
            .WithEnvironment("KC_HEALTH_ENABLED", "true")
            .WithEnvironment("KC_METRICS_ENABLED", "true")
            .WithEnvironment("KC_PROXY", "edge");

        // Importar realm na inicialização (apenas se especificado)
        if (!string.IsNullOrEmpty(options.ImportRealm))
        {
            keycloak = keycloak
                .WithEnvironment("KC_IMPORT", options.ImportRealm)
                .WithArgs("start", "--import-realm", "--optimized");
        }
        else
        {
            keycloak = keycloak.WithArgs("start", "--optimized");
        }

        // Em produção, usar HTTPS
        if (options.ExposeHttpEndpoint)
        {
            keycloak = keycloak.WithHttpsEndpoint(targetPort: 8443, name: "https");
        }

        var authUrl = options.ExposeHttpEndpoint
            ? $"https://localhost:{keycloak.GetEndpoint("https").Port}"
            : $"https://{options.Hostname ?? Environment.GetEnvironmentVariable("KEYCLOAK_HOSTNAME") ?? "change-me.example.com"}";
        var adminUrl = $"{authUrl}/admin";

        Console.WriteLine($"[Keycloak] ✅ Keycloak produção configurado:");
        Console.WriteLine($"[Keycloak]    Auth URL: {authUrl}");
        Console.WriteLine($"[Keycloak]    Admin URL: {adminUrl}");
        Console.WriteLine($"[Keycloak]    Schema: {options.DatabaseSchema}");

        return new MeAjudaAiKeycloakResult
        {
            Keycloak = keycloak,
            AuthUrl = authUrl,
            AdminUrl = adminUrl
        };
    }

    /// <summary>
    /// Adiciona configuração simplificada de Keycloak para testes
    /// </summary>
    public static MeAjudaAiKeycloakResult AddMeAjudaAiKeycloakTesting(
        this IDistributedApplicationBuilder builder,
        Action<MeAjudaAiKeycloakOptions>? configure = null)
    {
        var options = new MeAjudaAiKeycloakOptions
        {
            IsTestEnvironment = true,
            DatabaseSchema = "identity_test", // Schema separado para testes
            AdminPassword = "test123"
        };
        configure?.Invoke(options);

        Console.WriteLine($"[Keycloak] Configurando Keycloak para testes...");
        Console.WriteLine($"[Keycloak] Database Schema: {options.DatabaseSchema}");

        var keycloak = builder.AddKeycloak("keycloak-test")
            // Configurações otimizadas para teste
            .WithEnvironment("KC_DB", "postgres")
            .WithEnvironment("KC_DB_URL", $"jdbc:postgresql://{options.DatabaseHost}:{options.DatabasePort}/{options.DatabaseName}?currentSchema={options.DatabaseSchema}")
            .WithEnvironment("KC_DB_USERNAME", options.DatabaseUsername)
            .WithEnvironment("KC_DB_PASSWORD", options.DatabasePassword)
            .WithEnvironment("KC_DB_SCHEMA", options.DatabaseSchema)
            // Credenciais do admin
            .WithEnvironment("KEYCLOAK_ADMIN", options.AdminUsername)
            .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", options.AdminPassword)
            // Configurações simplificadas para velocidade
            .WithEnvironment("KC_HOSTNAME_STRICT", "false")
            .WithEnvironment("KC_HTTP_ENABLED", "true")
            .WithEnvironment("KC_HEALTH_ENABLED", "false")
            .WithEnvironment("KC_METRICS_ENABLED", "false")
            .WithEnvironment("KC_LOG_LEVEL", "WARN")
            .WithArgs("start-dev", "--db=postgres");

        keycloak = keycloak.WithHttpEndpoint(targetPort: 8080, name: "keycloak-test-http");

        var authUrl = $"http://localhost:{keycloak.GetEndpoint("keycloak-test-http").Port}";
        var adminUrl = $"{authUrl}/admin";

        Console.WriteLine($"[Keycloak] ✅ Keycloak teste configurado:");
        Console.WriteLine($"[Keycloak]    Auth URL: {authUrl}");
        Console.WriteLine($"[Keycloak]    Schema: {options.DatabaseSchema}");

        return new MeAjudaAiKeycloakResult
        {
            Keycloak = keycloak,
            AuthUrl = authUrl,
            AdminUrl = adminUrl
        };
    }
}