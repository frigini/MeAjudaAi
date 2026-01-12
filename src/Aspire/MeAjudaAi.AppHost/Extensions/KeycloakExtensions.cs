using MeAjudaAi.AppHost.Helpers;
using MeAjudaAi.AppHost.Options;
using MeAjudaAi.AppHost.Results;

namespace MeAjudaAi.AppHost.Extensions;

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
        var options = new MeAjudaAiKeycloakOptions
        {
            AdminUsername = string.Empty,
            AdminPassword = string.Empty
        };
        configure?.Invoke(options);

        if (string.IsNullOrWhiteSpace(options.AdminUsername) || string.IsNullOrWhiteSpace(options.AdminPassword))
        {
            throw new InvalidOperationException(
                "AdminUsername and AdminPassword must be configured for Keycloak. " +
                "Set via configuration callback or KEYCLOAK_ADMIN/KEYCLOAK_ADMIN_PASSWORD environment variables.");
        }

        // Validar senha do banco de dados em ambientes não-desenvolvimento
        var isDevelopment = EnvironmentHelpers.IsDevelopment(builder) || EnvironmentHelpers.IsTesting(builder);
        options.Validate(isDevelopment);

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
            AdminUsername = Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN") ?? "admin",
            ExposeHttpEndpoint = false,
            AdminPassword = adminPasswordFromEnv,
            DatabasePassword = dbPasswordFromEnv
        };
        configure?.Invoke(options);

        // Validar senha do banco de dados em produção (sempre false para este método)
        options.Validate(isDevelopment: false);

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

        // Definir KC_HOSTNAME quando usando hostname estrito (sem endpoint exposto)
        var resolvedHostname = options.Hostname ?? Environment.GetEnvironmentVariable("KEYCLOAK_HOSTNAME");
        if (!options.ExposeHttpEndpoint)
        {
            if (string.IsNullOrWhiteSpace(resolvedHostname))
                throw new InvalidOperationException("KEYCLOAK_HOSTNAME (ou options.Hostname) é obrigatório em produção com hostname estrito.");
            keycloak = keycloak.WithEnvironment("KC_HOSTNAME", resolvedHostname);
        }

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
            : $"https://{resolvedHostname}";
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
}
