namespace MeAjudaAi.AppHost.Options;

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
    /// SEGURANÇA: Configurar via User Secrets, variáveis de ambiente ou configuração segura.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Senha do PostgreSQL
    /// SEGURANÇA: Configurar via POSTGRES_PASSWORD, User Secrets ou configuração segura.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Indica se deve habilitar configuração otimizada para testes
    /// </summary>
    public bool IsTestEnvironment { get; set; }

    /// <summary>
    /// Indica se deve incluir PgAdmin para desenvolvimento
    /// </summary>
    public bool IncludePgAdmin { get; set; } = true;
}
