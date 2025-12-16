namespace MeAjudaAi.AppHost.Options;

/// <summary>
/// Opções de configuração para o setup do Keycloak do MeAjudaAi
/// </summary>
public sealed class MeAjudaAiKeycloakOptions
{
    /// <summary>
    /// Nome de usuário do administrador do Keycloak
    /// SEGURANÇA: Configurar via KEYCLOAK_ADMIN ou Configuration. Evitar defaults em produção.
    /// </summary>
    public string AdminUsername { get; set; } = string.Empty;

    /// <summary>
    /// Senha do administrador do Keycloak
    /// SEGURANÇA: Configurar via KEYCLOAK_ADMIN_PASSWORD ou Configuration. Evitar defaults em produção.
    /// </summary>
    public string AdminPassword { get; set; } = string.Empty;

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
    /// Senha do banco de dados PostgreSQL (OBRIGATÓRIO - configurar via variável de ambiente POSTGRES_PASSWORD)
    /// </summary>
    public string DatabasePassword { get; set; } =
        Environment.GetEnvironmentVariable("POSTGRES_PASSWORD")
        ?? throw new InvalidOperationException("POSTGRES_PASSWORD environment variable must be set for Keycloak database configuration");

    /// <summary>
    /// Hostname para URLs de produção (ex: keycloak.mydomain.com)
    /// </summary>
    public string? Hostname { get; set; }

    /// <summary>
    /// Indica se deve expor endpoint HTTP (padrão: true para desenvolvimento)
    /// </summary>
    public bool ExposeHttpEndpoint { get; set; } = true;

    /// <summary>
    /// Realm a ser importado na inicialização (configurar via KEYCLOAK_IMPORT_REALM se necessário)
    /// </summary>
    public string? ImportRealm { get; set; } = Environment.GetEnvironmentVariable("KEYCLOAK_IMPORT_REALM");

    /// <summary>
    /// Indica se está em ambiente de teste (configurações otimizadas)
    /// </summary>
    public bool IsTestEnvironment { get; set; }
}
