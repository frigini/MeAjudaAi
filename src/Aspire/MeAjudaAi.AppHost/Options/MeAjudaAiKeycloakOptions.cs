namespace MeAjudaAi.AppHost.Options;

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
